using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Bot.Controllers {

	[Route("api/[controller]")]
	public class ScheduleController : ControllerBase {

		private readonly ITelegramBotService telegramBotService;

		public ScheduleController(ITelegramBotService telegramBotService) => this.telegramBotService = telegramBotService;

		[HttpPost("{token}")]
		public async Task<IActionResult> SendSchedule([FromBody] Schedule schedule, string token) {
			if(telegramBotService.IsTokenCorrect(token)) {
				if(schedule.ScheduleType == ScheduleType.Day) {
					//await telegramBotService.SendDayScheduleAsync(schedule);
					throw new NotImplementedException();
				} else {
					//await telegramBotService.SendScheduleRangeAsync(schedule);
					throw new NotImplementedException();
				}
			}
			return Ok();
		}

	}

}