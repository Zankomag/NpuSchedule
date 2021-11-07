
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel.DataAnnotations;
using System.Linq;
using NpuSchedule.Common.Attributes;

namespace NpuSchedule.Bot.Configs {

	public class TelegramBotOptions {

		public const string SectionName = "TelegramBotClient";

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string Token { get; init;  }
		public long[] AdminIds { get; init;  }

		private long[] allowedChatIds;

		public long[] AllowedChatIds {
			get => allowedChatIds;
			set => allowedChatIds = value.Concat(AdminIds).Distinct().ToArray();
		}

		[Required]
		[FormatStringPlaceholderIndexesCount(1)]
		public string ScheduleClassInfoFieldTemplate { get; init; }
		
		[Required]
		[FormatStringPlaceholderIndexesCount(5)]
		public string ScheduleClassInfoMessageTemplate { get; init; }
		
		[Required]
		[FormatStringPlaceholderIndexesCount(7)]
		public string ScheduleClassMessageTemplate { get; init; }

		[Required]
		[FormatStringPlaceholderIndexesCount(3)]
		public string SingleScheduleDayMessageTemplate { get; init; }
		
		[Required]
		[FormatStringPlaceholderIndexesCount(3)]
		public string ScheduleDayMessageTemplate { get; init; }
		
		[Required]
		[FormatStringPlaceholderIndexesCount(4)]
		public string ScheduleWeekMessageTemplate { get; init; }

		[Required(AllowEmptyStrings = true)]
		public string ScheduleClassSeparator { get; init; }

		[Required(AllowEmptyStrings = true)]
		public string ScheduleDaySeparator { get; init; }
		
		[Required(AllowEmptyStrings = true)]
		public string ClassInfoSeparator { get; init; }
		
		[Required]
		public string NoClassesMessage { get; init; }
		
		[Required]
		public string IsRemoteClassMessage { get; init; }

		public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);
		public bool IsChatAllowed(long chatId) => AllowedChatIds.Contains(chatId);

	}

}