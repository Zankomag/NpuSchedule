using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models; 

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScheduleDay
{
	public DateTimeOffset Date { get; internal init; }
	public List<Class> Classes { get; internal init; }
}