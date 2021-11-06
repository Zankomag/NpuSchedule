using System;
using System.Threading.Tasks;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Core.Abstractions {

	public interface INpuScheduleService
	{

		/// <param name="endDate"></param>
		/// <param name="groupName">If groupName is null - default group name will be used</param>
		/// <param name="startDate"></param>
		/// <param name="maxScheduleDaysCount">The maximum quantity of schedule days to retrieve</param>
		Task<Schedule> GetSchedulesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string groupName = null, int maxScheduleDaysCount = Int32.MaxValue);

	}

}