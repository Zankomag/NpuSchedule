using System.Collections.Generic;

namespace NpuSchedule.Core.Models
{
    public class ScheduleDay
    {
        public string WeekName { get; internal set; }
        public string Date { get; internal set; }
        public List<Class> Classes { get; internal set; }
    }
}