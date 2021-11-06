using System;
using System.Threading.Tasks;
using NpuSchedule.Common.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Abstractions {
	
	public interface ITelegramBotService : IUpdateHandler {

		Task HandleMessageAsync(Message message);

		Task HandleInlineQueryAsync(InlineQuery inlineQuery);

		Task HandleUpdateAsync(Update update);

		bool IsTokenCorrect(string token);
		
		Task SendDayScheduleAsync(RelativeScheduleDay relativeScheduleDay, long chatId, string groupName = null);
		Task SendWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek, long chatId, string groupName = null);

	}

}