
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NpuSchedule.Bot.Configs {

	public class TelegramBotOptions {

		public const string SectionName = "TelegramBotClient";

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string Token { get; init;  }
		public long[] AdminIds { get; init;  }

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string RedStickerFileId { get; init;  }

		[Required]
		[RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
		public string GreenStickerFileId { get; init;  }

		[Required]
		[FormatStringPlaceholderIndexesCount(5)]
		public string CurrencyRateMarkdownMessageTemplate { get; init; }

		public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);

	}

}