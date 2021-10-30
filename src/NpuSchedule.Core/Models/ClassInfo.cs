namespace NpuSchedule.Core.Models
{
    /// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
    public class ClassInfo
    {
        public string Name { get; internal set; }
        public string Teacher { get; internal set; }
        public string Classroom { get; internal set; }
        public string UrlLink { get; internal set; }
    }
}