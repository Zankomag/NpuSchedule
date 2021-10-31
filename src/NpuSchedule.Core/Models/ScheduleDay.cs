using System;
using System.Collections.Generic;

namespace NpuSchedule.Core.Models
{
    public class ScheduleDay
    {
		public DateTime Date { get; init; }
        public List<Class> Classes { get; init; }
    }
}