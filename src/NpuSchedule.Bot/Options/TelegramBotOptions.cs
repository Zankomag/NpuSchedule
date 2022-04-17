using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using NpuSchedule.Common.Attributes;

namespace NpuSchedule.Bot.Options; 

public class TelegramBotOptions {

	public const string SectionName = "TelegramBotClient";

	[Required]
	[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
	public string Token { get; [UsedImplicitly] init;  }
	public long[] AdminIds { get; [UsedImplicitly] init;  }

	private long[] allowedChatIds;

	/// <summary>
	/// Also includes <see cref="AdminIds"/> 
	/// </summary>
	public long[] AllowedChatIds {
		get => allowedChatIds;
		set => allowedChatIds = value.Concat(AdminIds).Distinct().ToArray();
	}

	[Required]
	[FormatStringPlaceholderIndexesCount(1)]
	public string ScheduleClassInfoFieldTemplate { get; [UsedImplicitly] init; }
		
	[Required]
	[FormatStringPlaceholderIndexesCount(5)]
	public string ScheduleClassInfoMessageTemplate { get; [UsedImplicitly] init; }
		
	[Required]
	[FormatStringPlaceholderIndexesCount(7)]
	public string ScheduleClassMessageTemplate { get; [UsedImplicitly] init; }

	[Required]
	[FormatStringPlaceholderIndexesCount(3)]
	public string SingleScheduleDayMessageTemplate { get; [UsedImplicitly] init; }
		
	[Required]
	[FormatStringPlaceholderIndexesCount(3)]
	public string ScheduleDayMessageTemplate { get; [UsedImplicitly] init; }
		
	[Required]
	[FormatStringPlaceholderIndexesCount(4)]
	public string ScheduleWeekMessageTemplate { get; [UsedImplicitly] init; }

	[Required(AllowEmptyStrings = true)]
	public string ScheduleClassSeparator { get; [UsedImplicitly] init; }

	[Required(AllowEmptyStrings = true)]
	public string ScheduleDaySeparator { get; [UsedImplicitly] init; }
		
	[Required(AllowEmptyStrings = true)]
	public string ClassInfoSeparator { get; [UsedImplicitly] init; }
		
	[Required]
	public string NoClassesMessage { get; [UsedImplicitly] init; }
		
	[Required]
	public string IsRemoteClassMessage { get; [UsedImplicitly] init; }
		
	[Required]
	public string NpuSiteIsDownMessage { get; [UsedImplicitly] init; }

	public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);
	public bool IsChatAllowed(long chatId) => AllowedChatIds.Contains(chatId);

}