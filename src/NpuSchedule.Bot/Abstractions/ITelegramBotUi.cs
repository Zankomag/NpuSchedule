using System;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Bot.Abstractions {

	public interface ITelegramBotUi {

		string GetStatusMessage(DateTimeOffset startTime);

		string GetScheduleWeekMessage(Schedule schedule, DateTimeOffset startDate, DateTimeOffset endDate);

		string GetSingleScheduleDayMessage(ScheduleDay scheduleDay, DateTimeOffset date, string groupName);

	}

}