using System;
using System.Threading.Tasks;
using NpuSchedule.Core.Models;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Abstractions {
	
	public interface ITelegramBotService : IUpdateHandler {

		Task HandleUpdateAsync(Update update);

		bool IsTokenCorrect(string token);
		
		Task SendDayScheduleAsync(Schedule schedule, long chatId, DateTimeOffset startDate, DateTimeOffset endDate);
		Task SendScheduleRangeAsync(Schedule schedule, long chatId, DateTimeOffset startDate, DateTimeOffset endDate);

	}

}