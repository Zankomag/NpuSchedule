using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models;

/// <summary>
///     University class info (the time when students get together to listen to a lesson of a particular subject in
///     school)
/// </summary>
[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ClassInfo {

	public string? DisciplineName { get; private set; }
	public string? Teacher { get; private set; }
	public string? Classroom { get; private set; }
	public string? OnlineMeetingUrl { get; private set; }
	public bool IsRemote { get; private set; }

	internal ClassInfo(string? disciplineName, string? teacher, string? classroom, string? onlineMeetingUrl, bool isRemote) {
		DisciplineName = disciplineName;
		Teacher = teacher;
		Classroom = classroom;
		OnlineMeetingUrl = onlineMeetingUrl;
		IsRemote = isRemote;
	}

}