using System;

namespace NpuSchedule.Core.Models
{
    /// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class Class
    {
		public int Number { get; init; }
        public TimeSpan StartTime { get; init; }
        public TimeSpan EndTime { get; init; }
        public ClassInfo FirstClass { get; init; }
        public ClassInfo SecondClass { get; init; }
    }
}