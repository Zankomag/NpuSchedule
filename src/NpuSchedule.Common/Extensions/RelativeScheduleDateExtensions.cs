﻿using System;
using System.ComponentModel;
using NpuSchedule.Common.Enums;

namespace NpuSchedule.Common.Extensions {

	/// <summary>
	/// This class contain functions for dealing with Npu Schedule DateTimes
	/// </summary>
	public static class RelativeScheduleDateExtensions {

		private const string timeZoneId = "FLE Standard Time";
		
		//TODO use DateTimeOffset in data structures
		private static readonly TimeZoneInfo npuTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

		private static DateTimeOffset GetCurrentDateTimeOffset() 
			=> TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, npuTimeZone);

		private static DateTimeOffset AddDaysToDateTimeOffset(DateTimeOffset dateTimeOffset, double daysToAdd)
			=> TimeZoneInfo.ConvertTime(dateTimeOffset.ToUniversalTime().AddDays(daysToAdd), npuTimeZone);
		
		/// <summary>
		/// Normally returns zero-range date (for example 14/11 - 14/11) but in cases of weekends adds them to range
		/// </summary>
		public static (DateTimeOffset StartDateTimeOffset, DateTimeOffset EndDateTimeOffset) GetScheduleDateTimeOffsetRange(this RelativeScheduleDay scheduleDay) {
			var currentNpuDate = GetCurrentDateTimeOffset();
			return (scheduleDay, currentNpuDate.DayOfWeek) switch {
				(RelativeScheduleDay.Today, DayOfWeek.Saturday or DayOfWeek.Sunday) 
					=> (currentNpuDate, GetNextMondayDateTimeOffset(currentNpuDate)), //Range from Today to next Monday
				(RelativeScheduleDay.Today, _) 
					=> (currentNpuDate, currentNpuDate), //Range from Today to Today
				(RelativeScheduleDay.Tomorrow, DayOfWeek.Friday or DayOfWeek.Saturday) 
					=> (GetTomorrowDateTimeOffset(currentNpuDate), GetNextMondayDateTimeOffset(currentNpuDate)), //Range from Today to next Monday
				(RelativeScheduleDay.Tomorrow, _) 
					=> GetSingleDateTimeOffsetRange(GetTomorrowDateTimeOffset(currentNpuDate)), //Range from Tomorrow to Tomorrow
				(RelativeScheduleDay.Closest or _, _) => throw new InvalidEnumArgumentException(nameof(scheduleDay), (int)scheduleDay, typeof(RelativeScheduleDay))
			};
		}
		
		/// <exception cref="InvalidEnumArgumentException"></exception>
		public static (DateTimeOffset StartDateTimeOffset, DateTimeOffset EndDateTimeOffset) GetScheduleWeekDateTimeOffsetRange(this RelativeScheduleWeek scheduleWeek) {
			var currentNpuDate = GetCurrentDateTimeOffset();
			DateTimeOffset mondayDate = scheduleWeek switch {
				RelativeScheduleWeek.Current => GetNextMondayDateTimeOffset(currentNpuDate),
				RelativeScheduleWeek.Next => AddDaysToDateTimeOffset(currentNpuDate, GetDaysToNextMonday(currentNpuDate.DayOfWeek)),
				_ => throw new InvalidEnumArgumentException(nameof(scheduleWeek), (int)scheduleWeek, typeof(RelativeScheduleWeek))
			};
			return GetWeekDateTimeOffsetRange(mondayDate);
		}

		private static double GetDaysToCurrentMonday(DayOfWeek currentDayOfWeek) => -1d * ((7 + (currentDayOfWeek - DayOfWeek.Monday)) % 7);

		private static double GetDaysToNextMonday(DayOfWeek currentDayOfWeek) => GetDaysToCurrentMonday(currentDayOfWeek) + 7d;
		
		private static (DateTimeOffset StartDateTimeOffset, DateTimeOffset EndDateTimeOffset) GetWeekDateTimeOffsetRange(DateTimeOffset mondayDateTimeOffset)
		 => (mondayDateTimeOffset, AddDaysToDateTimeOffset(mondayDateTimeOffset, 7d));

		private static DateTimeOffset GetNextMondayDateTimeOffset(DateTimeOffset currentDate)
			=> AddDaysToDateTimeOffset(currentDate, GetDaysToCurrentMonday(currentDate.DayOfWeek));

		private static DateTimeOffset GetTomorrowDateTimeOffset(DateTimeOffset currentDate) => AddDaysToDateTimeOffset(currentDate, 1d);

		private static (DateTimeOffset, DateTimeOffset) GetSingleDateTimeOffsetRange(DateTimeOffset dateTimeOffset)
			=> (dateTimeOffset, dateTimeOffset); 
		
		public static bool IsNowDayTime() {
			int currentHour = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, npuTimeZone).Hour;
			return currentHour is >= 9 and <= 21;
		}

	}

}