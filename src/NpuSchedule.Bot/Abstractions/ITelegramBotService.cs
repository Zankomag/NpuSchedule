using System;
using System.Threading.Tasks;
using NpuSchedule.Bot.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Abstractions {
	
	public interface ITelegramBotService : IUpdateHandler {

		Task HandleMessageAsync(Message message);

		Task HandleInlineQueryAsync(InlineQuery inlineQuery);

		Task HandleUpdateAsync(Update update);

		bool IsTokenCorrect(string token);

		Task SendDaySchedule(DateTime date);
		Task SendDaySchedule(ScheduleDay scheduleDay);
		Task SendWeekSchedule(ScheduleWeek scheduleWeek);

	}

}