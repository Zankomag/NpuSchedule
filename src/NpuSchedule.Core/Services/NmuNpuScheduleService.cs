using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Configs;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Extensions;
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

		//TODO inject HttpClient
		private async Task<string> GetRawHtmlScheduleResponse(DateTime startDate, DateTime endDate, string groupName = null) {
			groupName ??= options.DefaultGroupName;
			
			var content = new Dictionary<string, string> {
				{ "sdate", startDate.ToString("dd.MM.yyyy") },
				{ "edate", endDate.ToString("dd.MM.yyyy") },
				{ "group", groupName },
			};
			var contentBytes = content.GetUrlEncodedContent().ToWindows1251();
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(@"http://nmu.npu.edu.ua");
			
			//n=700 should be as url parameter, otherwise it doesn't work
			var response = await client.PostAsync(@$"cgi-bin/timetable.cgi?n=700", new ByteArrayContent(contentBytes));

			var responseContentBytes = await response.Content.ReadAsByteArrayAsync();
			return responseContentBytes.FromWindows1251();
		}

	}

}