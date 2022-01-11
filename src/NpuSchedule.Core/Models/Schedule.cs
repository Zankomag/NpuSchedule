using System.Collections.Generic;

namespace NpuSchedule.Core.Models {

	public class Schedule {

		public string GroupName { get; init; }
		public IList<ScheduleDay> ScheduleDays { get; init; }

	}

}