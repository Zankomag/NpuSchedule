using System;
using System.Threading.Tasks;
using NpuSchedule.Core.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Abstractions {
	
	public interface ITelegramBotService : IUpdateHandler {

		Task HandleMessageAsync(Message message);

		Task HandleInlineQueryAsync(InlineQuery inlineQuery);

		Task HandleUpdateAsync(Update update);

		bool IsTokenCorrect(string token);

		Task SendDayScheduleAsync(DateTime date);
		Task SendDayScheduleAsync(RelativeScheduleDay relativeScheduleDay);
		Task SendWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek);

	}

}