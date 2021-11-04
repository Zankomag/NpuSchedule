using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
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
		private static readonly IBrowsingContext ParseContext = BrowsingContext.New();
		private const string TempDivider = "*|*";

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
			var response = await client.PostAsync(@"cgi-bin/timetable.cgi?n=700", new ByteArrayContent(contentBytes));

			var responseContentBytes = await response.Content.ReadAsByteArrayAsync();
			return responseContentBytes.FromWindows1251();
		}
		
		static async Task<List<ScheduleDay>> ParseRangeSchedule(string rawHtml, int maxCount = int.MaxValue)
		{
			var result = new List<ScheduleDay>();
			var document = await ParseContext.OpenAsync(r => r.Content(rawHtml));
			var daySelector = "div.container div.row div.col-md-6:not(.col-xs-12)";
			var days = document.QuerySelectorAll(daySelector);
			var maxLength = Math.Min(days.Length, maxCount);
			
			for (int i = 0; i < maxLength; i++)
				result.Add(ParseDaySchedule(days[i]));

			return result;
		}
		
		static ScheduleDay ParseDaySchedule(IElement rawDay)
		{
			var rawDate = rawDay.QuerySelector("h4")?.TextContent.Split(" ")[0];
			var date = DateTimeOffset.ParseExact(rawDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
			//TODO add check error parse
			
			var classes = new List<Class>();
			var rawClasses = rawDay.QuerySelectorAll("tr");

			for (int i = 0; i < rawClasses.Length; i++)
				if (rawClasses[i].InnerHtml.Contains("class=\"link\""))
					classes.Add(ParseClass(rawClasses[i]));
			
			return new ScheduleDay { Date = date, Classes = classes };
		}

		static Class ParseClass(IElement rawClass)
		{
			ClassInfo firstClass = null;
			ClassInfo secondClass = null;
			
			if(!int.TryParse(rawClass.QuerySelector("td:nth-child(1)")?.TextContent, out var numberClass))
				Console.WriteLine("Error get or parse number class"); //TODO replace console write to logger
			
			const int indexOfEndTime = 5;
			string timeClass = rawClass.QuerySelector("td:nth-child(2)")?.TextContent.Insert(indexOfEndTime, TempDivider);
			var rawTimeStartAndEnd = timeClass?.Split(TempDivider);
			var startTime = TimeSpan.Zero;
			var endTime = TimeSpan.Zero;
			
			if(rawTimeStartAndEnd?.Length < 1 || !TimeSpan.TryParse(rawTimeStartAndEnd?[0], out startTime))
				Console.WriteLine("Error get or parse start time"); //TODO replace console write to logger
			
			if(rawTimeStartAndEnd?.Length < 2 || !TimeSpan.TryParse(rawTimeStartAndEnd?[1], out endTime))
				Console.WriteLine("Error get or parse end time"); //TODO replace console write to logger

			var countClassInfo = rawClass.InnerHtml.CountSubstring("class=\"link\"");
			
			if (countClassInfo == 1)
				firstClass = ParseClassInfo(rawClass.QuerySelector("td:nth-child(3)"));
			else if (countClassInfo > 1)
			{
				var classInfo = rawClass.QuerySelector("td:nth-child(3)");
				
				if (classInfo != null)
				{
					const string endFirstClass = "</div> ";
					string tmp = classInfo.InnerHtml;
					var startIndex = classInfo.InnerHtml.IndexOf(endFirstClass, StringComparison.Ordinal);
						
					// remove second
					if (startIndex != -1)
						classInfo.InnerHtml = classInfo.InnerHtml[..(startIndex + endFirstClass.Length)];
					firstClass = ParseClassInfo(classInfo);

					// remove first and adaptation second classInfo for parser
					if (startIndex != -1)
						classInfo.InnerHtml = tmp[(startIndex + "</div> <br>".Length)..].Replace("  <div", "<br> <div");
					secondClass = ParseClassInfo(classInfo);
				}
			}
			
			return new Class { StartTime = startTime, EndTime = endTime, Number = numberClass ,FirstClass = firstClass, SecondClass = secondClass};
		}

		static ClassInfo ParseClassInfo(IElement classInfoObj)
		{
			var meetUrl = classInfoObj.QuerySelector("div.link a")?.InnerHtml;
			classInfoObj.InnerHtml = classInfoObj.InnerHtml.Replace(" ауд", TempDivider + "ауд");

			bool isRemote = false;
			if (classInfoObj.InnerHtml.Contains("class=\"remote_work\""))
			{
				const string endSpanRemote = "</span><br>";
				classInfoObj.InnerHtml = classInfoObj.InnerHtml
					[(classInfoObj.InnerHtml.IndexOf(endSpanRemote, StringComparison.Ordinal) + endSpanRemote.Length)..];
				isRemote = true;
			}

			var discipline = classInfoObj.ChildNodes[0].TextContent.Trim();
			string classroom = null;
			string teacher = null;

			var childIndex = classInfoObj.ChildNodes.Length switch
			{
				9 => // second string is data mixed group
					4,
				7 or 5 => // second string is teacher and maybe has classroom
					2,
				_ => -1
			};

			if (childIndex != -1)
			{
				if (childIndex == 4) discipline += $" ({classInfoObj.ChildNodes[2].TextContent.Trim()})";
				
				var rawTeacherAndClassroom = classInfoObj.ChildNodes[childIndex].TextContent;
				if (rawTeacherAndClassroom.Contains(TempDivider))
				{
					var teacherAndClassroom = rawTeacherAndClassroom.Split(TempDivider);
					teacher = teacherAndClassroom[0].Trim();
					classroom = teacherAndClassroom[1].Trim();
				}
				else
					teacher = rawTeacherAndClassroom.Trim();
			}
			
			return new ClassInfo {
				DisciplineName = discipline, Teacher = teacher, Classroom = classroom,
				OnlineMeetingUrl = meetUrl, IsRemote = isRemote
			};
		}
		
	}

}