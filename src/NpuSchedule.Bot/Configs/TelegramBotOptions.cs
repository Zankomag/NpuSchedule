
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

		//[Required]
		//[FormatStringPlaceholderIndexesCount(5)]
		//public string ScheduleDayMessageMessageTemplate { get; init; }

		public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);
		public bool IsChatAllowed(long chatId) => AllowedChatIds.Contains(chatId);

	}

}