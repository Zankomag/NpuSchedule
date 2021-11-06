namespace NpuSchedule.Core.Models
{
    /// <summary>University class info (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class ClassInfo
    {
        public string DisciplineName { get; set; }
        public string Teacher { get; set; }
        public string Classroom { get; set; }
        public string OnlineMeetingUrl { get; set; }
        public bool IsRemote { get; set; }
    }
}