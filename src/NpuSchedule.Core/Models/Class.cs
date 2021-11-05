using System;

namespace NpuSchedule.Core.Models
{
    /// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class Class
    {
		public int Number { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public ClassInfo FirstClass { get; set; }
        public ClassInfo SecondClass { get; set; }
    }
}