using System;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Bot.Abstractions {

	public interface ITelegramBotUi {

		string GetStatusMessage(DateTimeOffset? startTime = null);

		string GetScheduleWeekMessage(Schedule schedule, DateTimeOffset startDate, DateTimeOffset endDate);

		string GetSingleScheduleDayMessage(Schedule schedule, DateTimeOffset rangeEndDate, string groupName);

	}

}