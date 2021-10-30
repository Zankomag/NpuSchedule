using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Configs;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Core.Services {

	/// <summary>
	/// Gets schedule from NMU site
	/// </summary>
	public class NmuNpuScheduleService : INpuScheduleService {

		private readonly ILogger<NmuNpuScheduleService> logger;
		
		public NmuNpuScheduleService(IOptions<NpuScheduleOptions> options, ILogger<NmuNpuScheduleService> logger) {
			this.logger = logger;
		}

		public async Task<ScheduleDay> GetDayScheduleAsync(RelativeScheduleDay relativeScheduleDay)
		{
			throw new NotImplementedException();
		}

		public async Task<List<ScheduleDay>> GetWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek)
		{
			throw new NotImplementedException();
		}

		public async Task<List<ScheduleDay>> GetScheduleAsync(string rawHtmlCode)
		{
			var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
			var document = await context.OpenAsync(r => r.Content(rawHtmlCode));

			var cellSelector = "div.container div.row div.col-md-6:not(.col-xs-12)";
			var cells = document.QuerySelectorAll(cellSelector);
            
			return null;
		}
	}

}