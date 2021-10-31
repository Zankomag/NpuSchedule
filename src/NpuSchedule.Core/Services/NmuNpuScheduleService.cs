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

		private readonly NpuScheduleOptions options;
		private readonly ILogger<NmuNpuScheduleService> logger;
		
		public NmuNpuScheduleService(IOptions<NpuScheduleOptions> options, ILogger<NmuNpuScheduleService> logger) {
			this.options = options.Value;
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<ScheduleDay> GetDayScheduleAsync(RelativeScheduleDay relativeScheduleDay, string groupName = null)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public async Task<List<ScheduleDay>> GetWeekScheduleAsync(RelativeScheduleWeek relativeScheduleWeek, string groupName = null)
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

		/// <inheritdoc />
		public async Task<string> GetRawHtmlScheduleResponse(DateTime startDate, DateTime endDate, string groupName = null) => TODO_IMPLEMENT_ME;

	}

}