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
			var response = await client.PostAsync(@$"cgi-bin/timetable.cgi?n=700", new ByteArrayContent(contentBytes));

			var responseContentBytes = await response.Content.ReadAsByteArrayAsync();
			return responseContentBytes.FromWindows1251();
		}
		
		static async Task<List<ScheduleDay>> ParseRangeSchedule(string rawHtml)
        {
            var result = new List<ScheduleDay>();
            
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(r => r.Content(rawHtml));
            var daySelector = "div.container div.row div.col-md-6:not(.col-xs-12)";
            var days = document.QuerySelectorAll(daySelector);

            for (int i = 0; i < days.Length; i++)
                result.Add(ParseDaySchedule(days[i]));

            return result;
        }
        
        static ScheduleDay ParseDaySchedule(IElement rawDay)
        {
            var rawDate = rawDay.QuerySelector("h4")?.TextContent.Split(" ")[0];
            var date = DateTime.ParseExact(rawDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            
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
            int.TryParse(rawClass.QuerySelector("td:nth-child(1)")?.TextContent, out var numberClass);
            string timeClass = rawClass.QuerySelector("td:nth-child(2)")?.TextContent.Insert(5, " ");
            
            var startTime = TimeSpan.Parse(timeClass.Split(" ")[0]);
            var endTime = TimeSpan.Parse(timeClass.Split(" ")[1]);
            
            var countClassInfo = rawClass.InnerHtml.CountSubstring("class=\"link\"");
            if (countClassInfo == 1)
                firstClass = ParseClassInfo(rawClass.QuerySelector("td:nth-child(3)"));
            else if (countClassInfo > 1)
            {
                var classInfo = rawClass.QuerySelector("td:nth-child(3)");
                string tmp = classInfo?.InnerHtml;

                if (classInfo != null)
                {
                    var startIndex = classInfo.InnerHtml.IndexOf("</div> ", StringComparison.Ordinal);
                        
                    // remove second
                    if (startIndex != -1)
                        classInfo.InnerHtml = classInfo.InnerHtml[..(startIndex + 7)]; // 7 = "</div> ".Length
                    firstClass = ParseClassInfo(classInfo);

                    // remove first and adaptation second classInfo for parser
                    if (startIndex != -1)
                        classInfo.InnerHtml = tmp[(startIndex + 11)..].Replace("  <div", "<br> <div"); // 11 = "</div> ".Length + "<br>".Length
                    secondClass = ParseClassInfo(classInfo);
                }
            }
            
            return new Class { StartTime = startTime, EndTime = endTime, Number = numberClass ,FirstClass = firstClass, SecondClass = secondClass};
        }

        static ClassInfo ParseClassInfo(IElement classInfoObj)
        {
            string altDivider = "***";
            var meetUrl = classInfoObj.QuerySelector("div.link a")?.InnerHtml;
            classInfoObj.InnerHtml = classInfoObj.InnerHtml.Replace(" ауд", altDivider + "ауд");

            bool isRemote = false;
            if (classInfoObj.InnerHtml.Contains("class=\"remote_work\""))
            {
                classInfoObj.InnerHtml = classInfoObj.InnerHtml
	                [(classInfoObj.InnerHtml.IndexOf("</span><br>", StringComparison.Ordinal) + 11)..]; // 11 = "</span><br>".Length
                isRemote = true;
            }

            var discipline = classInfoObj.ChildNodes[0].TextContent.Trim();
            string classroom = null;
            string teacher = null;

            switch (classInfoObj.ChildNodes.Length)
            {
                case 9:
                    
                    if (classInfoObj.ChildNodes[4].TextContent.Contains(altDivider))
                    {
                        teacher = classInfoObj.ChildNodes[4].TextContent.Trim().Split(altDivider)[0];
                        classroom = classInfoObj.ChildNodes[4].TextContent.Trim().Split(altDivider)[1];
                    }
                    else
                        teacher = classInfoObj.ChildNodes[4].TextContent.Trim();
                    
                    break;
                case 7:
                case 5:
                    
                    if (classInfoObj.ChildNodes[2].TextContent.Contains(altDivider))
                    {
                        teacher = classInfoObj.ChildNodes[2].TextContent.Trim().Split(altDivider)[0];
                        classroom = classInfoObj.ChildNodes[2].TextContent.Trim().Split(altDivider)[1];
                    }
                    else
                        teacher = classInfoObj.ChildNodes[2].TextContent.Trim();
                    
                    break;
            }

            return new ClassInfo {
                DisciplineName = discipline, Teacher = teacher, Classroom = classroom,
                OnlineMeetingUrl = meetUrl, IsRemote = isRemote
            };
        }
        
	}

}