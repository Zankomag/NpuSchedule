
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
		
		//[Required]
		//[FormatStringPlaceholderIndexesCount(5)]
		//public string ScheduleDayMessageMessageTemplate { get; init; }

		public bool IsUserAdmin(long userId) => AdminIds.Contains(userId);

	}

}