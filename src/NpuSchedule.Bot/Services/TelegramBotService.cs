using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using NpuSchedule.Common.Enums;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Common.Utils;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Models;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace NpuSchedule.Bot.Services {
	
	public class TelegramBotService : ITelegramBotService {
		
		private readonly TelegramBotClient client;
		private readonly ILogger<TelegramBotService> logger;
		private readonly INpuScheduleService npuScheduleService;
		private readonly DateTime startTime;

		private readonly TelegramBotOptions options;
		private readonly string botUsername;

		/// <inheritdoc />
		public UpdateType[] AllowedUpdates { get; } = { UpdateType.Message, UpdateType.InlineQuery };

		public TelegramBotService(IOptions<TelegramBotOptions> telegramBotOptions, ILogger<TelegramBotService> logger, INpuScheduleService npuScheduleService) {
			this.logger = logger;
			this.npuScheduleService = npuScheduleService;
			options = telegramBotOptions.Value;
			client = new TelegramBotClient(options.Token);
			//TODO workaround this so it won't block
			botUsername = client.GetMeAsync().Result.Username;
			startTime = DateTime.UtcNow;
		}

		public async Task HandleMessageAsync(Message message) {
			//bot doesn't read old messages to avoid /*spam*/ 
			//2 minutes threshold due to slow start of aws lambda
			if(message.Date < startTime.AddMinutes(-2)) return;

			//If command contains bot username we need to exclude it from command (/btc@MyBtcBot should be /btc)
			int atIndex = message.Text.IndexOf('@');
			
			//Bot should not respond to commands in group chats without direct mention
			if(message.From.Id != message.Chat.Id && atIndex != -1 && message.Text[(atIndex + 1)..] != botUsername) 
				return;
			
			string command = atIndex == -1 ? message.Text : message.Text[..atIndex];

			//TODO refactor allowed chat:)
			//Command handler has such a simple and dirty implementation because telegram bot is really simple and made mostly for demonstration purpose
			switch(command.ToLower()) {
				case "/today":
					if(options.IsChatAllowed(message.Chat.Id)) {
						await SendDayScheduleAsync(RelativeScheduleDay.Today, message.Chat.Id);
					}
					break;
				case "/tomorrow":
					if(options.IsChatAllowed(message.Chat.Id)) {
						await SendDayScheduleAsync(RelativeScheduleDay.Tomorrow, message.Chat.Id);
					}
					break;
				case "/closest":
					if(options.IsChatAllowed(message.Chat.Id)) {
						await SendDayScheduleAsync(RelativeScheduleDay.Closest, message.Chat.Id);
					}
					break;
				case "/week":
					if(options.IsChatAllowed(message.Chat.Id)) {
						await SendWeekScheduleAsync(RelativeScheduleWeek.Current, message.Chat.Id);
					}
					break;
				case "/nextweek":
					if(options.IsChatAllowed(message.Chat.Id)) {
						await SendWeekScheduleAsync(RelativeScheduleWeek.Next, message.Chat.Id);
					}
					break;
				case "/health":
					if(message.Chat.Id == message.From.Id && options.IsUserAdmin(message.From.Id)) {
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

		/// <inheritdoc />
		public async Task SendDayScheduleAsync(DateTime date, long chatId) => throw new NotImplementedException();

		/// <inheritdoc />
		public async Task SendDayScheduleAsync(RelativeScheduleDay relativeScheduleDay, long chatId, string groupName = null) {
			
			try {
				string message;
				(DateTimeOffset startDate, DateTimeOffset endDate) = relativeScheduleDay.GetScheduleDateTimeOffsetRange();
				var schedule = await npuScheduleService.GetSchedulesAsync(startDate, endDate, groupName, 1);
				if(schedule.ScheduleDays.Count == 1) {
					message = GetSingleScheduleDayMessage(schedule.ScheduleDays[0], schedule.ScheduleDays[0].Date, groupName);
				} else {
					message = GetScheduleWeekMessage(schedule.ScheduleDays, startDate, endDate, groupName);
				}
				await client.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
			} catch(Exception ex) {
				logger.LogError(ex, "Received exception while sending schedule message");
			}
		}

		/// <inheritdoc />
		public async Task SendWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek, long chatId, string groupName = null) => throw new NotImplementedException();

		//TODO Move all message getters to Ui service
		//TODO add groupName to new shedule type
		private string GetScheduleWeekMessage(IList<ScheduleDay> scheduleDays, DateTimeOffset startDate, DateTimeOffset endDate, string groupName) {
			
			string scheduleWeekDays;
			if(scheduleDays == null || scheduleDays.Count == 0) {
				scheduleWeekDays = options.NoClassesMessage;
			} else {
				StringBuilder scheduleDayClassesBuilder = new StringBuilder();
				for(int i = 0; i < scheduleDays.Count; i++) {
					var scheduleDay = scheduleDays[i];
					scheduleDayClassesBuilder.AppendFormat(options.ScheduleDayMessageTemplate,
						scheduleDay.Date,
						GET_CLASSES);
				}
				scheduleWeekDays = scheduleDayClassesBuilder.ToString();
			}
			return String.Format(options.ScheduleWeekMessageTemplate, startDate, endDate, groupName, scheduleWeekDays);
		}
		
//TODO Add exception logging for day and week
		private string GetSingleScheduleDayMessage(ScheduleDay scheduleDay, DateTimeOffset date,  string groupName) {
			
			string scheduleDayClasses;
			if(scheduleDay?.Classes?.Any() != true) {
				scheduleDayClasses = options.NoClassesMessage;
			} else {
				scheduleDayClasses = GetScheduleDayClassesMessage(scheduleDay);
			}
			return String.Format(options.SingleScheduleDayMessageTemplate, date, groupName, scheduleDayClasses);
		}

		private string GetScheduleDayClassesMessage(ScheduleDay scheduleDay) {
			StringBuilder scheduleDayClassesBuilder = new StringBuilder();
			for(int i = 0; i < scheduleDay.Classes.Count; i++) {
				var @class = scheduleDay.Classes[i];
				scheduleDayClassesBuilder.AppendFormat(options.ScheduleClassMessageTemplate,
					@class.Number,
					@class.StartTime,
					@class.EndTime,
					GetClassInfoMessage(@class.FirstClass),
					@class.SecondClass != null ? options.ClassInfoSeparator : null,
					@class.SecondClass != null ? GetClassInfoMessage(@class.SecondClass) : null,
					i < scheduleDay.Classes.Count - 1 ? options.ScheduleClassSeparator : null);
			}
			return scheduleDayClassesBuilder.ToString();
		}
		
		private string GetClassInfoMessage(ClassInfo classInfo)
			=> String.Format(options.ScheduleClassInfoMessageTemplate,
					GetClassInfoField(classInfo.DisciplineName),
					GetClassInfoField(classInfo.Teacher),
					GetClassInfoField(classInfo.Classroom),
					GetClassInfoField(classInfo.OnlineMeetingUrl));

		private string GetClassInfoField(string classInfoField)
			=> classInfoField != null ? String.Format(options.ScheduleClassInfoFieldTemplate, classInfoField) : null;

		// //TODO delete this
		// private ScheduleDay scheduleDay = new ScheduleDay() {
		// 	Date = DateTime.Now,
		// 	Classes = new List<Class> {
		// 		new Class() {
		// 			StartTime = new TimeSpan(12, 30, 0),
		// 			EndTime = new TimeSpan(14, 50, 0),
		// 			Number = 1,
		// 			FirstClass = new ClassInfo() {
		// 				Classroom = "urban central",
		// 				DisciplineName = "math",
		// 				OnlineMeetingUrl = null,
		// 				Teacher = "Savina"
		// 			},
		// 			SecondClass = null
		// 		},
		// 		new Class() {
		// 			StartTime = new TimeSpan(16, 0, 0),
		// 			EndTime = new TimeSpan(17, 20, 0),
		// 			Number = 3,
		// 			FirstClass = new ClassInfo() {
		// 				Classroom = null,
		// 				DisciplineName = "ukr",
		// 				OnlineMeetingUrl = "https://meet.com",
		// 				Teacher = "Savina"
		// 			},
		// 			SecondClass = new ClassInfo() {
		// 				Classroom = "urban central",
		// 				DisciplineName = "math",
		// 				OnlineMeetingUrl = null,
		// 				Teacher = "Savina"
		// 			}
		// 		}
		// 	}
		// };

		
	}

}