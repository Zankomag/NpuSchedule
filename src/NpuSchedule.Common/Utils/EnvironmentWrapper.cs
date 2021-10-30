using System;

namespace NpuSchedule.Common.Utils {

	public static class EnvironmentWrapper {

		public const string EnvironmentName = nameof(EnvironmentName);

		public const string Development = nameof(Development);

		public static bool IsDevelopment => GetEnvironmentName() == Development;

		public static string GetEnvironmentName() => Environment.GetEnvironmentVariable(EnvironmentName) ?? Development;

	}

}