using System;
using System.Collections.Generic;

namespace NpuSchedule.Core.Models
{
    public class ScheduleDay
    {
		public DateTime Date { get; internal set; }
        public List<Class> Classes { get; internal set; }
    }
}