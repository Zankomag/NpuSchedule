using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Common.Utils;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Controllers {

	[Route("api/[controller]")]
	public class BotUpdateController : ControllerBase {

		private readonly ITelegramBotService telegramBotService;
		private readonly ITelegramBotUi telegramBotUi;

		public BotUpdateController(ITelegramBotService telegramBotService, ITelegramBotUi telegramBotUi) {
			this.telegramBotService = telegramBotService;
			this.telegramBotUi = telegramBotUi;
		}

		[HttpPost("{token}")]
		public async Task<IActionResult> PostUpdate([FromBody] Update update, string token) {
			if(telegramBotService.IsTokenCorrect(token)) {
				await telegramBotService.HandleUpdateAsync(update);
			}
			return Ok();
		}
		
		[HttpGet("status")]
		[HttpGet("healthCheck")]
		public string HealthCheck() => telegramBotUi.GetStatusMessage();

	}

}