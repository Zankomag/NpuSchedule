namespace NpuSchedule.Core.Models
{
    /// <summary>University class info (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class ClassInfo
    {
        public string DisciplineName { get; internal set; }
        public string Teacher { get; internal set; }
        public string Classroom { get; internal set; }
        public string OnlineMeetingUrl { get; internal set; }
    }
}