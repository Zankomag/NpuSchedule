namespace NpuSchedule.Core.Models
{
    /// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class Class
    {
        public int Number { get; internal set; }
        public string StartTime { get; internal set; }
        public string EndTime { get; internal set; }
        public ClassInfo FirstClass { get; internal set; }
        public ClassInfo SecondClass { get; internal set; }
    }
}