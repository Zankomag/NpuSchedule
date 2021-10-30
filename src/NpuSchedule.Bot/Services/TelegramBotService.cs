using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using NpuSchedule.Common.Utils;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace NpuSchedule.Bot.Services {

//TODO add inline mode for getting rate in any chat
	public class TelegramBotService : ITelegramBotService {
		
		private readonly TelegramBotClient client;
		private readonly ILogger<TelegramBotService> logger;
		private readonly DateTime startTime;

		private readonly TelegramBotOptions options;
		private readonly string botUsername;

		/// <inheritdoc />
		public UpdateType[] AllowedUpdates { get; } = { UpdateType.Message, UpdateType.InlineQuery };

		public TelegramBotService(IOptions<TelegramBotOptions> telegramBotOptions, ILogger<TelegramBotService> logger) {
			this.logger = logger;
			options = telegramBotOptions.Value;
			client = new TelegramBotClient(options.Token);
			//TODO workaround this so it won't block
			botUsername = client.GetMeAsync().Result.Username;
			startTime = DateTime.UtcNow;
		}

		public async Task HandleMessageAsync(Message message) {
			//bot doesn't read old messages to avoid /*spam*/ 
			// DISABLED DUE TO LAMBDA ISSUES
			//TODO fix end enable
			//if(message.Date < startTime) return;

			//If command contains bot username we need to exclude it from command (/btc@MyBtcBot should be /btc)
			int atIndex = message.Text.IndexOf('@');
			
			//Bot should not respond to commands in group chats without direct mention
			if(message.From.Id != message.Chat.Id && atIndex != -1 && message.Text[(atIndex + 1)..] != botUsername) 
				return;
			
			string command = atIndex == -1 ? message.Text : message.Text[..atIndex];

			//Command handler has such a simple and dirty implementation because telegram bot is really simple and made mostly for demonstration purpose
			switch(command.ToLower()) {
				case "/today":
					
					break;
				case "/tomorrow":
					
					break;
				case "/health":
					if(options.IsUserAdmin(message.From.Id)) {
						await client.SendTextMessageAsync(message.From.Id, $"Running\nEnvironment: {EnvironmentWrapper.GetEnvironmentName()}\ndotnet {Environment.Version}\nstart time: {startTime}");
					}
					break;
			}
		}

		public Task HandleInlineQueryAsync(InlineQuery inlineQuery) => Task.CompletedTask;

		/// <inheritdoc />
		async Task IUpdateHandler.HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => await HandleUpdateAsync(update);

		public async Task HandleUpdateAsync(Update update) {
			switch(update.Type) {
				case UpdateType.Message:
					if(update.Message.Type == MessageType.Text) await HandleMessageAsync(update.Message);
					break;
				case UpdateType.InlineQuery:
					await HandleInlineQueryAsync(update.InlineQuery);
					break;
				default: logger.LogWarning("Update type {update.Type} is not supported", update.Type);
					break;
			}

		}

		/// <inheritdoc />
		public Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			logger.LogError(exception, "Received an exception from Telegram Bot API");
			return Task.CompletedTask;
		}

		public bool IsTokenCorrect(string token) => token != null && token == options.Token;

	}

}