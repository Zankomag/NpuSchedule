using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NpuSchedule.Core.Configs {

	public class NpuScheduleOptions {

		public const string SectionName = "NpuSchedule";

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string DefaultGroupName { get; init; }

	}

}