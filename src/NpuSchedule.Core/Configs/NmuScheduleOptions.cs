using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NpuSchedule.Core.Configs {

	public class NmuScheduleOptions {

		public const string SectionName = "NmuSchedule";

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string DefaultGroupName { get; init; }

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string NmuAddress { get; init; }

	}

}