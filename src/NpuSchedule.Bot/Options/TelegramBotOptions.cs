using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using NpuSchedule.Common.Attributes;

namespace NpuSchedule.Bot.Options; 

public class TelegramBotOptions {

	public const string SectionName = "TelegramBotClient";

	[Required, RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
	public string Token { get; [UsedImplicitly] init;  } = null!;

	public long[] AdminIds { get; [UsedImplicitly] init; } = Array.Empty<long>();

	private long[] allowedChatIds = Array.Empty<long>();

	/// <summary>
	/// Also includes <see cref="AdminIds"/> 
	/// </summary>
	public long[] AllowedChatIds {
		get => allowedChatIds;
		set => allowedChatIds = value.Concat(AdminIds).Distinct().ToArray();
	}

	[Required,FormatStringPlaceholderIndexesCount(1)]
	public string ScheduleClassInfoFieldTemplate { get; [UsedImplicitly] init; } = null!;
		
	[Required,FormatStringPlaceholderIndexesCount(5)]
	public string ScheduleClassInfoMessageTemplate { get; [UsedImplicitly] init; } = null!;
		
	[Required,FormatStringPlaceholderIndexesCount(7)]
	public string ScheduleClassMessageTemplate { get; [UsedImplicitly] init; } = null!;

	[Required,FormatStringPlaceholderIndexesCount(3)]
	public string SingleScheduleDayMessageTemplate { get; [UsedImplicitly] init; } = null!;
		
	[Required,FormatStringPlaceholderIndexesCount(3)]
	public string ScheduleDayMessageTemplate { get; [UsedImplicitly] init; } = null!;
		
	[Required,FormatStringPlaceholderIndexesCount(4)]
	public string ScheduleWeekMessageTemplate { get; [UsedImplicitly] init; } = null!;

	[Required(AllowEmptyStrings = true)]
	public string ScheduleClassSeparator { get; [UsedImplicitly] init; } = null!;

	[Required(AllowEmptyStrings = true)]
	public string ScheduleDaySeparator { get; [UsedImplicitly] init; } = null!;
		
	[Required(AllowEmptyStrings = true)]
	public string ClassInfoSeparator { get; [UsedImplicitly] init; } = null!;
		
	[Required]
	public string NoClassesMessage { get; [UsedImplicitly] init; } = null!;
		
	[Required]
	public string IsRemoteClassMessage { get; [UsedImplicitly] init; } = null!;
		
	[Required]
	public string NpuSiteIsDownMessage { get; [UsedImplicitly] init; } = null!;

	public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);
	public bool IsChatAllowed(long chatId) => AllowedChatIds.Contains(chatId);

}