using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models; 

/// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Class
{
	public int Number { get; internal init; }
	public TimeSpan StartTime { get; internal init; }
	public TimeSpan EndTime { get; internal init; }
	public ClassInfo FirstClass { get; internal init; }
	public ClassInfo? SecondClass { get; internal init; }
}