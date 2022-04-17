using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace NpuSchedule.Core.Options; 

public class NmuScheduleOptions {

	public const string SectionName = "NmuSchedule";

	[Required]
	[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
	public string DefaultGroupName { get; [UsedImplicitly] init; }

	[Required]
	[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
	public string NmuAddress { get; [UsedImplicitly] init; }

}