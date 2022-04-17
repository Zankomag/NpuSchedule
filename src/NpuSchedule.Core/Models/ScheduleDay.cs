using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ScheduleDay {

	public DateTimeOffset Date { get; private set; }
	public List<Class> Classes { get; private set; }

	internal ScheduleDay(DateTimeOffset date, List<Class> classes) {
		Date = date;
		Classes = classes;
	}

}