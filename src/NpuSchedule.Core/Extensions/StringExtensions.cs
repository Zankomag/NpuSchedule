﻿using System.Text;

namespace NpuSchedule.Core.Extensions {

	public static class StringExtensions {

		private static Encoding windows1251Encoding;

		private static Encoding Windows1251Encoding {
			get {
				if(windows1251Encoding != null) {
					return windows1251Encoding;
				}
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				windows1251Encoding = Encoding.GetEncoding(1251);
				return windows1251Encoding;
			}
		}
		
		public static byte[] ToWindows1251(this string @string) => Windows1251Encoding.GetBytes(@string);

		public static string FromWindows1251(this byte[] bytes) => Windows1251Encoding.GetString(bytes);

		public static int CountSubstring(this string value, string substring)
		{
			if (string.IsNullOrEmpty(substring))
				return 0;
			return (value.Length - value.Replace(substring, string.Empty).Length) / substring.Length;
		}

	}

}