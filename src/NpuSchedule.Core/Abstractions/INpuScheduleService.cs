using System.Collections.Generic;
using System.Threading.Tasks;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Core.Abstractions {

	public interface INpuScheduleService
	{
		Task<ScheduleDay> GetDayScheduleAsync(RelativeScheduleDay relativeScheduleDay);
		
		Task<List<ScheduleDay>> GetWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek);

		Task<List<ScheduleDay>> GetScheduleAsync(string rawHtmlCode);
	}

}