using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Core.Abstractions {

	public interface INpuScheduleService
	{

		/// <param name="endDate"></param>
		/// <param name="groupName">If groupName is null - default group name will be used</param>
		/// <param name="startDate"></param>
		Task<List<ScheduleDay>> GetSchedulesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string groupName = null);
	}

}