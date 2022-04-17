using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Extensions;
using NpuSchedule.Core.Models;
using NpuSchedule.Core.Options;

namespace NpuSchedule.Core.Services; 

/// <summary>
///     Gets schedule from Nmu site
/// </summary>
public class NmuNpuScheduleService : INpuScheduleService {

	private const string classFieldsSeparator = "*|*";

	private readonly NmuScheduleOptions options;
	private readonly ILogger<NmuNpuScheduleService> logger;
	private readonly IBrowsingContext browsingContext;
	private readonly HttpClient nmuClient;
	
	//n=700 should be as url parameter, otherwise it doesn't work
	const string scheduleRequestUri = @"cgi-bin/timetable.cgi?n=700";
	private readonly Uri fullScheduleRequestUri;

	public NmuNpuScheduleService(IOptions<NmuScheduleOptions> options, ILogger<NmuNpuScheduleService> logger,
		IBrowsingContext browsingContext, IHttpClientFactory httpClientFactory) {
		this.options = options.Value;
		this.logger = logger;
		this.browsingContext = browsingContext;

		nmuClient = httpClientFactory.CreateClient();
		nmuClient.BaseAddress = new Uri(this.options.NmuAddress);
		fullScheduleRequestUri = new Uri(nmuClient.BaseAddress, scheduleRequestUri);
	}

	/// <inheritdoc />
	public async Task<Schedule> GetSchedulesAsync(DateTimeOffset startDate, DateTimeOffset endDate, string? groupName = null, int maxScheduleDaysCount = Int32.MaxValue) {
		groupName ??= options.DefaultGroupName;
		if(String.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("Parameter must not be null or whitespace", nameof(groupName));
		string rawHtml = await GetRawHtmlScheduleResponse(startDate, endDate, groupName);
		var scheduleDays = await ParseRangeSchedule(rawHtml, maxScheduleDaysCount);

		return new Schedule(groupName, scheduleDays);
	}

	private async Task<string> GetRawHtmlScheduleResponse(DateTimeOffset startDate, DateTimeOffset endDate, string groupName) {
		var content = new Dictionary<string, string> {
			{ "sdate", startDate.ToString("dd.MM.yyyy") },
			{ "edate", endDate.ToString("dd.MM.yyyy") },
			{ "group", groupName }
		};
		var contentBytes = content.GetUrlEncodedContent().ToWindows1251();

		string rawHtml;

		try {
			var response = await nmuClient.PostAsync(scheduleRequestUri, new ByteArrayContent(contentBytes));
			if(response.IsSuccessStatusCode) {
				var responseContentBytes = await response.Content.ReadAsByteArrayAsync();
				rawHtml = responseContentBytes.FromWindows1251().Replace("windows-1251", "utf-8");
			} else {
				throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode}");
			}
		} catch(HttpRequestException ex) {
			logger.LogError(ex, "Exception thrown during request to {Uri}", fullScheduleRequestUri);
			throw;
		} catch(TaskCanceledException ex) {
			logger.LogError(ex, "Exception thrown during request to {Uri}: Site response timed out", fullScheduleRequestUri);
			throw;
		} catch(Exception ex) {
			logger.LogError(ex, "Unhandled exception thrown while handling web response");
			throw;
		}
		return rawHtml;
	}

	private async Task<IList<ScheduleDay>> ParseRangeSchedule(string rawHtml, int maxCount = Int32.MaxValue) {
		if(String.IsNullOrWhiteSpace(rawHtml)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(rawHtml));

		const string daySelector = "div.container div.row div.col-md-6:not(.col-xs-12)";
		var document = await browsingContext.OpenAsync(r => r.Content(rawHtml));
		var days = document.QuerySelectorAll(daySelector);
		var maxLength = Math.Min(days.Length, maxCount);
		var scheduleDays = new List<ScheduleDay>(maxLength);

		for(int i = 0; i < maxLength; i++) {
			scheduleDays.Add(ParseDaySchedule(days[i]));
		}

		return scheduleDays;
	}

	private ScheduleDay ParseDaySchedule(IParentNode rawDay) {
		var rawDate = rawDay.QuerySelector("h4")?.TextContent.Split(" ")[0];
		DateTimeOffset date;
		try {
			date = DateTimeOffset.ParseExact(rawDate!, "dd.MM.yyyy", CultureInfo.InvariantCulture);
		} catch(Exception ex) {
			logger.LogError(ex, "Exception thrown when parsing date");
			throw;
		}

		var rawClasses = rawDay.QuerySelectorAll("tr");
		var classes = rawClasses.Where(x => x.InnerHtml.Contains("class=\"link\""))
			.Select(ParseClass)
			.ToList();

		return new ScheduleDay(date, classes);
	}

	private Class ParseClass(IElement rawClass) {
		ClassInfo firstClass;
		ClassInfo? secondClass = null;
		int numberClass;
		try {
			numberClass = Int32.Parse(rawClass.QuerySelector("td:nth-child(1)")?.TextContent!);
		} catch(Exception ex) {
			logger.LogError(ex, "Exception thrown when parsing numberClass");
			throw;
		}

		const int indexOfEndTime = 5;
		string? timeClass = rawClass.QuerySelector("td:nth-child(2)")?.TextContent.Insert(indexOfEndTime, classFieldsSeparator);
		var rawStartAndEndTime = timeClass?.Split(classFieldsSeparator) 
			?? throw new InvalidOperationException("Unable to retrieve class Start and End time from schedule html");
		TimeSpan startTime;
		TimeSpan endTime;

		try {
			startTime = TimeSpan.Parse(rawStartAndEndTime[0]);
			endTime = TimeSpan.Parse(rawStartAndEndTime[1]);
		} catch(Exception ex) {
			logger.LogError(ex, "Exception thrown when parsing StartAndEnd class time");
			throw;
		}

		var countClassInfo = rawClass.InnerHtml.CountSubstring("class=\"link\"");

		switch(countClassInfo) {
			case 1:
				IElement firstClassElement = rawClass.QuerySelector("td:nth-child(3)")
					?? throw new InvalidOperationException("Unable to retrieve first class from schedule html");
				firstClass = ParseClassInfo(firstClassElement);
				break;
			case > 1: {
				IElement classInfoElement = rawClass.QuerySelector("td:nth-child(3)")
					?? throw new InvalidOperationException("Unable to retrieve ClassInfo from schedule html");

				const string endFirstClass = "</div> ";
				string tmp = classInfoElement.InnerHtml;
				var startIndex = classInfoElement.InnerHtml.IndexOf(endFirstClass, StringComparison.Ordinal);

				// remove second
				if(startIndex != -1)
					classInfoElement.InnerHtml = classInfoElement.InnerHtml[..(startIndex + endFirstClass.Length)];
				firstClass = ParseClassInfo(classInfoElement);


				// remove first and adaptation second classInfo for parser
				if(startIndex != -1)
					classInfoElement.InnerHtml = tmp[(startIndex + "</div> <br>".Length)..].Replace("  <div", "<br> <div");
				secondClass = ParseClassInfo(classInfoElement);

				break;
			}
			default: throw new InvalidOperationException("Unable to retrieve ClassInfo from schedule html");
		}

		return new Class(numberClass,
			startTime: startTime,
			endTime: endTime,
			firstClass: firstClass,
			secondClass: secondClass);
	}

	private ClassInfo ParseClassInfo(IElement classInfoObj) {
		var meetUrl = classInfoObj.QuerySelector("div.link a")?.InnerHtml;
		classInfoObj.InnerHtml = classInfoObj.InnerHtml.Replace(" ауд", classFieldsSeparator + "ауд");

		bool isRemote = false;
		if(classInfoObj.InnerHtml.Contains("class=\"remote_work\"")) {
			const string endSpanRemote = "</span><br>";
			var indexEndSpanRemote = classInfoObj.InnerHtml.IndexOf(endSpanRemote, StringComparison.Ordinal);

			if(indexEndSpanRemote != -1)
				classInfoObj.InnerHtml = classInfoObj.InnerHtml[(indexEndSpanRemote + endSpanRemote.Length)..];

			isRemote = true;
		}

		var discipline = classInfoObj.ChildNodes[0].TextContent.Trim();
		string? classroom = null;
		string? teacher = null;

		var childIndex = classInfoObj.ChildNodes.Length switch {
			9 => 4, // second string is data mixed group
			7 or 5 => 2, // second string is teacher and maybe has classroom
			_ => -1
		};

		// Handling a rare case when the node count < 9 but the 4th is not empty.
		if(childIndex == 2 && !String.IsNullOrWhiteSpace(classInfoObj.ChildNodes[4].TextContent))
			childIndex = 4;

		if(childIndex != -1) {
			if(childIndex == 4) discipline += $" ({classInfoObj.ChildNodes[2].TextContent.Trim()})";

			var rawTeacherAndClassroom = classInfoObj.ChildNodes[childIndex].TextContent;
			if(rawTeacherAndClassroom.Contains(classFieldsSeparator)) {
				var teacherAndClassroom = rawTeacherAndClassroom.Split(classFieldsSeparator);
				teacher = teacherAndClassroom[0].Trim();
				classroom = teacherAndClassroom[1].Trim();
			} else {
				teacher = rawTeacherAndClassroom.Trim();
			}
		}

		return new ClassInfo(discipline, teacher, classroom, meetUrl, isRemote);
	}

}