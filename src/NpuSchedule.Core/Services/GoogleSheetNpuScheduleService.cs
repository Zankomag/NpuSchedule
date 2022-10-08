using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
		if(availableEndDate.Date < startDate.Date || availableStartDate.Date > endDate.Date)
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
					Class = item.ClassInfo.EffectiveValue.StringValue.ToString().Replace("\n", " "),
					item.ClassInfo.Hyperlink
				})
				.Where(x => {
					// We're comparing x.Date.Date here instead of x.Date because:
					// x.Date >= startDate.Date call DatetimeOffset overloaded operator which 
					// compares 2 date time offsets. Since startDate.Date is not Datetime offset, 
					// but just a datetime taken from its DateTimeOffset ClockDateTime (so it's utc DateTime + Offset)
					// it is a startDate in NPU timezone
					// After that it's converted to DateTimeOffset by implicit operator (DateTime => DateTimeOffset)
					// with default constructor: new DateTimeOffset(DateTime) 
					// which treats given DateTime as local. Since it's local and constructor create a DateTimeOffset - 
					// it needs a determine an offset. And it take local machine timezone offset. Here's where magic happens:
					// It calculates absolute UTC value of DateTimeOffset data structure by subtracting local machine timezone offset 
					// from given DateTime value. And given DateTime value is ClockDateTime value from NPU timezone. 
					// Therefore after subtracting - UTC value of new structure can be the same startDate's ONLY if 
					// local machine timezone is same as NPU! Otherwise if local machine timezone offset is differentfrom NPU timezone
					// - when it comes to comparing two DateTimeOffsets:
					// left side (which is x.Date) - is just fine.
					// It has NPU timezone offset, therefore it's internal UTC value (absoluteUtcValue) is NPU timezone offset lesser than its ClockDateTime. 
					// BUT for right side (which is 'new DateTimeOffset(startDate.Date) { absoluteUtcValue = startDate.Date - localMachineTimezoneOffset;  }
					// So if, for example local machine timezone is lesser than NPU timezone - comparison will fail.  
					//
					// Here's an example of (return x.Date >= startDate.Date && x.Date <= endDate.Date;):
					//
					// Local machine timezone is UTC:
					// x.Date.ToUniversalTime(): 10/9/2022 9:00:00 PM + 00:00 
					// x.Date.Date.ToUniversalTime(): 10/10/2022 12:00:00 AM 
					// startDate.ToUniversalTime(): 10/10/2022 1:31:27 AM + 00:00 
					// startDate.Date.ToUniversalTime(): 10/10/2022 12:00:00 AM 
					//
					// Local machine timezone is same as NPU timezone:
					// x.Date.ToUniversalTime(): 10/9/2022 9:00:00 PM + 00:00 
					// x.Date.Date.ToUniversalTime(): 10/9/2022 9:00:00 PM 
					// startDate.ToUniversalTime(): 10/10/2022 1:31:27 AM + 00:00 
					// startDate.Date.ToUniversalTime(): 10/9/2022 9:00:00 PM 
					//
					// As we can see - we cannot compare x.Date and startDate because startDate is ahead, therefore comparison will fail
					// on "Local machine timezone is UTC" example we can see that it treated startDate.Date => DateTimeOffset => ToUniversalTime() 3 hours greater than 
					// x.Date.ToUniversalTime(), while it shouldn't. That's because actual startDate.Date value is 10/10/2022 12:00:00 AM (which is correct),
					// but since local machine timezone offset is 00:00 - it subtracted 0 from 10/10/2022 12:00:00 AM. While x.Date.ToUniversalTime()
					// subtracted 03:00 (NPU timezone) from 10/10/2022 12:00:00 AM and got 10/9/2022 9:00:00 PM
					// And on "Local machine timezone is same as NPU timezone" example we see that since local machine timezone offset whas 03:00 - it
					// subtracted this from 10/10/2022 12:00:00 AM (startDate.Date) and got 10/9/2022 9:00:00 PM which is the same as x.Date.ToUniversalTime()
					
					// ReSharper disable once ConvertToLambdaExpression
					return x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date;
				})
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
		
		
		DateTime endDateRaw = DateTime.ParseExact(dateRange[1], "dd.MM", null);
		// On this stage we have a pure date time, but since we work with NPU time zone, 
		// we need to wrap it as NPU timezone, since if we try to parse it right to DateTimeOffset 
		// when our Local timezone is different from NPU - it will result in being lesser in UTC then 
		// requested border start and end dateTimeOffsets 
		//
		// Take an example:
		// requested start DateTimeOffset is 2022-10-10 01:00:00 +03:00 
		// application machine's local timezone is UTC (+00:00)
		// dataRange[1] is "10.10"
		// available end DateTimeOffset will be 2022-10-10 00:00:00 +00:00
		// when comparing requested start date and available end date, then converted to UTC, therefore:
		// requested start date will be: 2022-10-09 22:00:00
		// available end date will be: 2022-10-10 00:00:00
		// meaning that while without offset we have requested and available ranges overlapping with one day,
		// technically we cannot provide any schedule for requested date as by absolute UTC value they differ with one day by Date parameter
		// this wouldn't happen if available end DateTimeOffset was 2022-10-10 00:00:00 +03:00
		DateTimeOffset endDate = new DateTimeOffset(endDateRaw, RelativeScheduleDateExtensions.NpuTimeZone.GetUtcOffset(endDateRaw));
			
		
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