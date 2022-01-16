using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using NpuSchedule.Bot.Extensions;
using NpuSchedule.Common.Enums;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace NpuSchedule.Bot.Services {

	public class TelegramBotService : ITelegramBotService {

		private readonly ITelegramBotClient client;
		private readonly ILogger<TelegramBotService> logger;
		private readonly INpuScheduleService npuScheduleService;
		private readonly ITelegramBotUi botUi;
		private readonly DateTimeOffset startTime;
		private readonly TelegramBotOptions options;
		
		/// <summary>
		/// Bot username with @ in front
		/// </summary>
		private readonly Lazy<Task<string>> botUsername;

		/// <inheritdoc />
		public UpdateType[] AllowedUpdates { get; } = { UpdateType.Message, UpdateType.InlineQuery };

		public TelegramBotService(IOptions<TelegramBotOptions> telegramBotOptions, ILogger<TelegramBotService> logger, INpuScheduleService npuScheduleService, ITelegramBotUi botUi) {
			this.logger = logger;
			this.npuScheduleService = npuScheduleService;
			this.botUi = botUi;
			options = telegramBotOptions.Value;
			client = new TelegramBotClient(options.Token);
			botUsername = new Lazy<Task<string>>(async () => await InitializeBotUsername());
			startTime = DateTimeOffset.UtcNow.ConvertToNpuTimeZone();
		}

		private async Task<string> InitializeBotUsername() {
			var botInfo = await client.GetMeAsync();
			return String.Concat('@', botInfo.Username);
		}

		Task IUpdateHandler.HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => HandleUpdateAsync(update);

		public async Task HandleUpdateAsync(Update update) {
			switch(update.Type) {
				case UpdateType.Message:
					if(update.Message.Type == MessageType.Text) await HandleMessageAsync(update.Message);
					break;
				case UpdateType.InlineQuery:
					await HandleInlineQueryAsync(update.InlineQuery);
					break;
				default:
					logger.LogWarning("Update type {update.Type} is not supported", update.Type);
					break;
			}

		}

		/// <inheritdoc />
		public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			logger.LogError(exception, "Received an exception from Telegram Bot API");
			return Task.CompletedTask;
		}

		public bool IsTokenCorrect(string token) => token != null && token == options.Token;
		
		private async Task SendDayScheduleAsync(RelativeScheduleDay relativeScheduleDay, long chatId, string groupName = null) {
			(DateTimeOffset startDate, DateTimeOffset endDate) = relativeScheduleDay.GetScheduleDateTimeOffsetRange();
			var schedule = await npuScheduleService.GetSchedulesAsync(startDate, endDate, groupName, 1);
			await SendDayScheduleAsync(schedule, chatId, startDate, endDate);
		}

		/// <inheritdoc />
		public async Task SendDayScheduleAsync(Schedule schedule, long chatId, DateTimeOffset startDate, DateTimeOffset endDate) {
			try {
				string message;
				if(schedule.ScheduleType == ScheduleType.Day) {
					message = botUi.GetSingleScheduleDayMessage(schedule, endDate, schedule.GroupName);
				} else {
					message = botUi.GetScheduleWeekMessage(schedule, startDate, endDate);
				}
				await client.SendTextMessageWithRetryAsync(chatId, message, ParseMode.Markdown, disableWebPagePreview: true);
			} catch(HttpRequestException ex) {
				logger.LogError(ex, "Received exception while sending day schedule message");
			} catch(TaskCanceledException) {
				try {
					await client.SendTextMessageWithRetryAsync(chatId, options.NpuSiteIsDownMessage);
				} catch(Exception ex2) {
					logger.LogError(ex2, "Received exception while sending telegram message");
				}
			} catch(Exception ex) {
				logger.LogError(ex, "Received exception while sending day schedule message");
			}
		}

		private async Task SendScheduleRangeAsync(RelativeScheduleWeek relativeScheduleWeek, long chatId, string groupName = null) {
			(DateTimeOffset startDate, DateTimeOffset endDate) = relativeScheduleWeek.GetScheduleWeekDateTimeOffsetRange();
			var schedule = await npuScheduleService.GetSchedulesAsync(startDate, endDate, groupName);
			await SendScheduleRangeAsync(schedule, chatId, startDate, endDate);
		}

		/// <inheritdoc />
		public async Task SendScheduleRangeAsync(Schedule schedule, long chatId, DateTimeOffset startDate, DateTimeOffset endDate) {
			try {
				string message = botUi.GetScheduleWeekMessage(schedule, startDate, endDate);
				await client.SendTextMessageWithRetryAsync(chatId, message, ParseMode.Markdown, disableWebPagePreview: true);
			} catch(Exception ex) {
				logger.LogError(ex, "Received exception while sending single schedule message");
			}
		}

		private async Task HandleMessageAsync(Message message) {
			if(message is null) throw new ArgumentNullException(nameof(message));

			//bot doesn't read old messages to avoid /*spam*/ 
			//2 minutes threshold due to slow start of aws lambda
			if(message.Date < startTime.AddMinutes(-2)) return;

			//If command contains bot username we need to exclude it from command (/btc@MyBtcBot should be /btc)
			string username = await botUsername.Value;
			int botMentionIndex = message.Text.IndexOf(username, StringComparison.Ordinal);
			int spaceIndex = message.Text.IndexOf(' ');

			//There should not be spaces between @botUsername and /command (should be as /command@botUsername). Also space cannot be first char
			if((spaceIndex != -1 && botMentionIndex > spaceIndex) || spaceIndex == 0)
				return;

			//Bot should not respond to commands in group chats without direct mention
			if(message.From.Id != message.Chat.Id && botMentionIndex == -1)
				return;

			(string command, string arg) = SplitMessagePayload(message, botMentionIndex, spaceIndex);
			
			//Command handler has such a simple and dirty implementation because telegram bot is really simple and made mostly for demonstration purpose
			if(options.IsChatAllowed(message.Chat.Id)) {
				switch(command.ToLower()) {
					case "/today":
						await SendDayScheduleAsync(RelativeScheduleDay.Today, message.Chat.Id, arg);
						break;
					case "/tomorrow":
						await SendDayScheduleAsync(RelativeScheduleDay.Tomorrow, message.Chat.Id, arg);
						break;
					case "/closest":
						await SendDayScheduleAsync(RelativeScheduleDay.Closest, message.Chat.Id, arg);
						break;
					case "/week":
						await SendScheduleRangeAsync(RelativeScheduleWeek.Current, message.Chat.Id, arg);
						break;
					case "/nextweek":
						await SendScheduleRangeAsync(RelativeScheduleWeek.Next, message.Chat.Id, arg);
						break;
					case "/health":
					case "/version":
					case "/status":
						if(message.Chat.Id == message.From.Id && options.IsUserAdmin(message.From.Id)) {
							await client.SendTextMessageWithRetryAsync(message.From.Id, botUi.GetStatusMessage(startTime));
						}
						break;
				}
			}
		}

		/// <summary>
		/// Splits message by command and single arg after command if exists
		/// </summary>
		/// <param name="message"></param>
		/// <param name="botMentionIndex">An index of @botUsername</param>
		/// <param name="spaceIndex">A first appearance of space in message</param>
		/// <returns></returns>
		private static (string command, string arg) SplitMessagePayload(Message message, int botMentionIndex, int spaceIndex) {
			//This implementation calculates only single arg (all text after command). To calculate arg list changes needed
			(string command, string arg) = (botMentionIndex, spaceIndex) switch {
				(-1, -1) => (message.Text, null),
				(_, -1) => (message.Text[..botMentionIndex], null),
				(-1, _) => (message.Text[..spaceIndex], message.Text[spaceIndex..]),
				(_, _) => (message.Text[..botMentionIndex], message.Text[spaceIndex..])
			};
			return (command, arg);
		}

		private Task HandleInlineQueryAsync(InlineQuery inlineQuery) {
			if(inlineQuery is null) throw new ArgumentNullException(nameof(inlineQuery));
			return Task.CompletedTask;
		}

	}

}