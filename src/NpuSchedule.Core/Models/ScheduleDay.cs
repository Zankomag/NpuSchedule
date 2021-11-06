using System;
using System.Collections.Generic;

namespace NpuSchedule.Core.Models
{
    public class ScheduleDay
    {
		public DateTimeOffset Date { get; set; }
        public List<Class> Classes { get; set; }
    }
}