using System.Collections.Generic;
using NpuSchedule.Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NpuSchedule.Core.Models {

	[JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class Schedule {

		public string GroupName { get; init; }
		public IList<ScheduleDay> ScheduleDays { get; init; }

		[JsonIgnore]
		public ScheduleType ScheduleType {
			get {
				if(ScheduleDays.Count == 1)
					return ScheduleType.Day;
				return ScheduleType.DateRange;
			}

		}

	}

}