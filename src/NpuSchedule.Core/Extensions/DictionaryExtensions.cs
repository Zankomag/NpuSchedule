using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable UseDeconstruction


namespace NpuSchedule.Core.Extensions {

	public static class DictionaryExtensions {

		public static string GetUrlEncodedContent(this Dictionary<string, string> valueByName) {
			if(valueByName == null) {
				throw new ArgumentNullException(nameof(valueByName));
			}

			StringBuilder builder = new StringBuilder();
			foreach(KeyValuePair<string, string> pair in valueByName) {
				if(builder.Length > 0) {
					builder.Append('&');
				}

				builder.Append(pair.Key);
				builder.Append('=');
				builder.Append(pair.Value);
			}
			return builder.ToString();
		}

	}

}