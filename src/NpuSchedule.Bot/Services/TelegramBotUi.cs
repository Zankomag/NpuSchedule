using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using NpuSchedule.Common.Utils;
using NpuSchedule.Core.Models;

namespace NpuSchedule.Bot.Services {

	public class TelegramBotUi : ITelegramBotUi {

		private readonly TelegramBotOptions options;

		public TelegramBotUi(IOptions<TelegramBotOptions> options) => this.options = options.Value;

		public string GetStatusMessage(DateTimeOffset startTime) => $"Version: {Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}"
			+ $"\nEnvironment: {EnvironmentWrapper.GetEnvironmentName()}"
			+ $"\ndotnet {Environment.Version}"
			+ $"\nStart time: {startTime:dd/MM/yyyy HH:mm:ss zz}";

		public string GetScheduleWeekMessage(Schedule schedule, DateTimeOffset startDate, DateTimeOffset endDate) {

			string scheduleWeekDays;
			if(schedule.ScheduleDays == null || schedule.ScheduleDays.Count == 0) {
				scheduleWeekDays = options.NoClassesMessage;
			} else {
				StringBuilder scheduleDayClassesBuilder = new();
				foreach(ScheduleDay scheduleDay in schedule.ScheduleDays) {
					scheduleDayClassesBuilder.AppendFormat(options.ScheduleDayMessageTemplate,
						scheduleDay.Date,
						GetScheduleDayClassesMessage(scheduleDay), options.ScheduleDaySeparator);
				}
				scheduleWeekDays = scheduleDayClassesBuilder.ToString();
			}
			return String.Format(options.ScheduleWeekMessageTemplate, startDate, endDate, schedule.GroupName, scheduleWeekDays);
		}

		public string GetSingleScheduleDayMessage(ScheduleDay scheduleDay, DateTimeOffset date, string groupName) {

			string scheduleDayClasses;
			if(scheduleDay?.Classes?.Any() != true) {
				scheduleDayClasses = options.NoClassesMessage;
			} else {
				scheduleDayClasses = GetScheduleDayClassesMessage(scheduleDay);
			}
			return String.Format(options.SingleScheduleDayMessageTemplate, date, groupName, scheduleDayClasses);
		}

		private string GetScheduleDayClassesMessage(ScheduleDay scheduleDay) {
			StringBuilder scheduleDayClassesBuilder = new();
			for(int i = 0; i < scheduleDay.Classes.Count; i++) {
				var @class = scheduleDay.Classes[i];
				scheduleDayClassesBuilder.AppendFormat(options.ScheduleClassMessageTemplate,
					@class.Number,
					@class.StartTime,
					@class.EndTime,
					GetClassInfoMessage(@class.FirstClass),
					@class.SecondClass != null ? options.ClassInfoSeparator : null,
					@class.SecondClass != null ? GetClassInfoMessage(@class.SecondClass) : null,
					i < scheduleDay.Classes.Count - 1 ? options.ScheduleClassSeparator : null);
			}
			return scheduleDayClassesBuilder.ToString();
		}

		private string GetClassInfoMessage(ClassInfo classInfo)
			=> String.Format(options.ScheduleClassInfoMessageTemplate,
				GetClassInfoField(classInfo.DisciplineName),
				GetClassInfoField(classInfo.Teacher),
				GetClassInfoField(classInfo.Classroom),
				GetClassInfoField(classInfo.OnlineMeetingUrl),
				classInfo.IsRemote ? options.IsRemoteClassMessage : null);

		private string GetClassInfoField(string classInfoField)
			=> classInfoField != null ? String.Format(options.ScheduleClassInfoFieldTemplate, classInfoField) : null;

	}

}