using System.ComponentModel.DataAnnotations;

namespace NpuSchedule.Core.Options {

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