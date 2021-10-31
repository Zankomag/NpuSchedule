using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Core.Abstractions {

	public interface INpuScheduleService
	{
		/// <param name="groupName">If groupName is null - default group name will be used</param>
		Task<ScheduleDay> GetDayScheduleAsync(RelativeScheduleDay relativeScheduleDay, string groupName = null);

		/// <param name="groupName">If groupName is null - default group name will be used</param>
		Task<List<ScheduleDay>> GetWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek, string groupName = null);

		Task<List<ScheduleDay>> GetScheduleAsync(string rawHtmlCode);

		/// <param name="groupName">If groupName is null - default group name will be used</param>
		Task<string> GetRawHtmlScheduleResponse(DateTime startDate, DateTime endDate, string groupName = null);

	}

}