using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models;

/// <summary>University class (the time when students get together to listen to a lesson of a particular subject in school)</summary>
[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Class {

	public int Number { get; private set; }
	public TimeSpan StartTime { get; private set; }
	public TimeSpan EndTime { get; private set; }
	public ClassInfo FirstClass { get; private set; }
	public ClassInfo? SecondClass { get; private set; }

	internal Class(int number, TimeSpan startTime, TimeSpan endTime, ClassInfo firstClass, ClassInfo? secondClass) {
		Number = number;
		StartTime = startTime;
		EndTime = endTime;
		FirstClass = firstClass;
		SecondClass = secondClass;
	}

}