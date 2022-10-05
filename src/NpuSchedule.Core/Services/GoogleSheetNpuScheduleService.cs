using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Models;
using NpuSchedule.Core.Options;

namespace NpuSchedule.Core.Services; 

public class GoogleSheetNpuScheduleService : INpuScheduleService {

	private readonly HttpClient httpClient;
	private readonly ILogger<GoogleSheetNpuScheduleService> logger;
	private readonly GoogleSheetScheduleOptions options;
	private readonly SheetsService sheetsService;
	
	public GoogleSheetNpuScheduleService(HttpClient httpClient, ILogger<GoogleSheetNpuScheduleService> logger, IOptions<GoogleSheetScheduleOptions> options, SheetsService sheetsService) {
		this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.sheetsService = sheetsService ?? throw new ArgumentNullException(nameof(sheetsService));
		this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

		this.httpClient.BaseAddress = new Uri($"https://sheets.googleapis.com/v4/spreadsheets/{this.options.GoogleSheetId}");
	}
	
	/// <inheritdoc />
	public async Task<Schedule> GetSchedulesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string? groupName = null, int maxScheduleDaysCount = Int32.MaxValue) {
		var spreadsheet = await sheetsService.Spreadsheets.Get(options.GoogleSheetId).ExecuteAsync();
		var sheetTitle = spreadsheet.Sheets.First(x => x.Properties.Title.StartsWith(options.GraduationLevel, StringComparison.Ordinal)).Properties.Title;

		(DateTimeOffset availableStartDate, DateTimeOffset availableEndDate) = GetWeeklySheetDateTimeRange(sheetTitle);
		if(availableEndDate < startDate || availableStartDate > endDate)
			return new Schedule(options.GroupName, new List<ScheduleDay>(), startDate, endDate);

		
		
		
		// google sheets api library doesn't allow to retrieve hyperlink or any other cell data except text,
		// this library call allows us to retrieve only text values:
		//
		// var cells = await sheetsService.Spreadsheets.Values.Get(options.GoogleSheetId, $"{sheetTitle}!{options.GoogleSheetGroupColumnLetter}3:{options.GoogleSheetGroupColumnLetter}74").ExecuteAsync();
		//
		// so we make custom call to google sheets api selecting fields we need
		Uri requestUri = new Uri(httpClient.BaseAddress!, $"?key={this.options.GoogleApiKey}&fields=sheets(data(rowData(values(hyperlink,effectiveValue(stringValue)))))"
			+ $"&ranges={sheetTitle}!{options.GoogleSheetGroupColumnLetter}3:{options.GoogleSheetGroupColumnLetter}74");
		
		var response = await httpClient.GetAsync(requestUri).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();
		var googleSheetCellList = await response.Content.ReadFromJsonAsync<GoogleSheetCellList>();
		
		if(googleSheetCellList?.Sheets?.Count > 0
			&& googleSheetCellList.Sheets[0].Data?.Count > 0
			&& googleSheetCellList.Sheets[0].Data![0].RowData?.Count > 2
			&& googleSheetCellList.Sheets[0].Data![0].RowData![1].Values![0].EffectiveValue.StringValue.StartsWith(options.GroupName, StringComparison.Ordinal)) {

			var result = googleSheetCellList.Sheets[0].Data![0].RowData!
				.Skip(2)
				.Where((_, index) => index % 2 == 0)
				.Select((rawItem, index) => {
					if(rawItem.Values?.Count > 0) {
						return new {
							Date = availableStartDate.AddDays(index / 7 + 1 - 1),
							DailyClassIndex = index - 7 * (index / 7) + 1,
							ClassInfo = rawItem.Values![0]
						};
					}
					return null;
				})
				.Where(x => x is not null)
				.Select(item => new {
					item!.Date,
					item.DailyClassIndex,
					Class = item!.ClassInfo.EffectiveValue.StringValue.ToString()!.Replace("\n", " "),
					item!.ClassInfo.Hyperlink
				})
				.Where(x => x.Date >= startDate.Date && x.Date <= endDate.Date)
				.GroupBy(key => key.Date)
				.Take(maxScheduleDaysCount)
				.Select(rawClasses => {
					List<Class> classes = new List<Class>(2);
					foreach(var classInfo in rawClasses) {
						var classFields = classInfo.Class!.Split(',');
						string className = classFields[0].Trim();
						string? teacher = classFields.Length > 1 && !String.IsNullOrWhiteSpace(classFields[1]) ? classFields[1].Trim() : null;

						(TimeSpan startTime, TimeSpan endTime) = GetClassStartAndEndTimeFromClassIndex(classInfo.DailyClassIndex);

						classes.Add(new Class(classInfo.DailyClassIndex, startTime, endTime,
							new ClassInfo(className, teacher, null, classInfo.Hyperlink, false), null));
					}

					return new ScheduleDay(rawClasses.Key, classes);
				})
				.ToList();

			if(result.Count > 0)
				return new Schedule(options.GroupName, result,
					result.First().Date, result.Last().Date);

			return new Schedule(options.GroupName, result,
				availableStartDate, availableEndDate);
			
		}

		throw new Exception($"Wrong group on column {options.GoogleSheetGroupColumnLetter}");
	}

	private (DateTimeOffset startDate, DateTimeOffset endDate) GetWeeklySheetDateTimeRange(string sheetTitle) {
		var dateRange = sheetTitle.Split(' ')[1].Split('-');
		
		DateTimeOffset endDate = DateTimeOffset.ParseExact(dateRange[1], "dd.MM", null);
		endDate = endDate.ConvertToNpuTimeZone();
		
		if(endDate.DayOfWeek != DayOfWeek.Friday)
			throw new Exception($"End date day of week should be Friday, but was {endDate.DayOfWeek}");
		
		//Get Monday from Friday
		var startDate = endDate.AddDays(-4);

		return (startDate, endDate);
	}

	private (TimeSpan startTime, TimeSpan endTime) GetClassStartAndEndTimeFromClassIndex(int classIndex)
		=> classIndex switch {
			1 => (new TimeSpan(8, 0, 0), new TimeSpan(9, 20, 0)),
			2 => (new TimeSpan(9, 30, 0), new TimeSpan(10, 50, 0)),
			3 => (new TimeSpan(11, 0, 0), new TimeSpan(12, 20, 0)),
			4 => (new TimeSpan(12, 30, 0), new TimeSpan(13, 50, 0)),
			5 => (new TimeSpan(14, 0, 0), new TimeSpan(15, 20, 0)),
			6 => (new TimeSpan(15, 30, 0), new TimeSpan(16, 50, 0)),
			7 => (new TimeSpan(17, 0, 0), new TimeSpan(18, 20, 0)),
			_ => throw new ArgumentOutOfRangeException(nameof(classIndex), classIndex, null)
		};

}