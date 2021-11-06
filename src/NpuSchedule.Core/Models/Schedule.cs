using System.Collections.Generic;

namespace NpuSchedule.Core.Models {

	public class Schedule {

		public string GroupName { get; set; }
		public IList<ScheduleDay> ScheduleDays { get; set; }

	}

}