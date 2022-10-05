using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NpuSchedule.Core.Enums;

namespace NpuSchedule.Core.Models;

[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Schedule {

	public DateTimeOffset StartDate { get; set; }
	public DateTimeOffset EndDate { get; set; }
	public string GroupName { get; private set; }
	public IList<ScheduleDay> ScheduleDays { get; private set; }

	[JsonIgnore]
	public ScheduleType ScheduleType {
		get {
			if(ScheduleDays.Count == 1)
				return ScheduleType.Day;
			return ScheduleType.DateRange;
		}

	}

	internal Schedule(string groupName, IList<ScheduleDay> scheduleDays, DateTimeOffset startDate, DateTimeOffset endDate) {
		GroupName = groupName;
		ScheduleDays = scheduleDays;
		StartDate = startDate;
		EndDate = endDate;
	}

}