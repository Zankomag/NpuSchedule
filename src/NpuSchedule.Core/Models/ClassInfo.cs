using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models; 

/// <summary>University class info (the time when students get together to listen to a lesson of a particular subject in school)</summary>
[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ClassInfo
{
    public string DisciplineName { get; internal init; }
    public string? Teacher { get; internal init; }
    public string? Classroom { get; internal init; }
    public string? OnlineMeetingUrl { get; internal init; }
    public bool IsRemote { get; internal init; }
}