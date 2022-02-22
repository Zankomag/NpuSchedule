using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NpuSchedule.Common.Attributes {

	/// <summary>
	///     Validates if a template for String.Format contains required quantity of variable placeholders ({0}, {1} etc.)
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class FormatStringPlaceholderIndexesCountAttribute : ValidationAttribute {

		// "index" is hardcoded because interpolated string will be hard to read
		// because of double { } that interpolated string requires for escape of { }
		// since regex is mainly about { } as well
		private static readonly Regex placeholderRegex = new(@"(?<!{)(?:{{)*{(?<index>\d+)(?:,-?\d+)?(?::[\s\S-[{}]]*)?}(?:}})*(?!})");
		private static readonly Regex doubleCurlyBracesRegex = new(@"(({{)|(}}))+");
		private readonly int placeholderIndexesCount;
		private readonly string propertyName;


		/// <inheritdoc />
		public FormatStringPlaceholderIndexesCountAttribute(int placeholderIndexesCount, [CallerMemberName] string propertyName = "property") {
			if(placeholderIndexesCount < 0)
				throw new ArgumentException("value must be greater or equal to 0", nameof(placeholderIndexesCount));
			this.placeholderIndexesCount = placeholderIndexesCount;
			this.propertyName = propertyName;
		}
		
		protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
			if(value is not string format)
				return ValidationResult.Success;

			var matches = placeholderRegex.Matches(format);
			var placeholderIndexes = matches.Select(x => x.Groups["index"].Value);
			List<int> validIndexes = new List<int>(matches.Count);

			foreach(string indexString in placeholderIndexes) {
				if(!Int32.TryParse(indexString, out int index)) {
					return new ValidationResult($"Index value is too big. Unable to parse index {{{indexString}}} of {propertyName} to Int32");
				}
				if(index >= placeholderIndexesCount) {
					return new ValidationResult(
						$"Placeholder index {{{indexString}}} of {propertyName} is greater than excepted. Format string must not contain indexes greater than {placeholderIndexesCount - 1}");
				}
				validIndexes.Add(index);
			}
			int validIndexesCount = validIndexes.Distinct().OrderBy(x => x).Count();
			if(validIndexesCount != placeholderIndexesCount)
				return new ValidationResult($"Quantity of format placeholder indexes {{{validIndexesCount}}} doesn't match expected ({placeholderIndexesCount}) in {propertyName}");

			return AreUnmatchedSubstringsValid(format, matches);
		}

		/// <summary>
		///     Checks if unmatched to regex substrings contain curvy braces that are unescaped and are not a part of a placeholder
		/// </summary>
		private ValidationResult AreUnmatchedSubstringsValid(string format, MatchCollection matches) {
			const string validationErrorFormat = @"{0} contains {{ or }} that are not a part of a placeholder and are not escaped";
			foreach(var substring in GetUnmatchedRegexSubstrings(format, matches)) {
				var doubleBraceMatches = doubleCurlyBracesRegex.Matches(substring);
				if(doubleBraceMatches.Count != 0) {
					if(GetUnmatchedRegexSubstrings(substring, doubleBraceMatches).Any(StringContainsCurlyBraces)) {
						return new ValidationResult(String.Format(validationErrorFormat, propertyName));
					}
				} else {
					if(StringContainsCurlyBraces(substring)) {
						return new ValidationResult(String.Format(validationErrorFormat, propertyName));
					}
				}
			}
			return ValidationResult.Success;
		}

		private static bool StringContainsCurlyBraces(string @string) => @string.Any(x => x is '{' or '}');

		private static IEnumerable<string> GetUnmatchedRegexSubstrings(string format, MatchCollection matches) {
			if(matches.Count != 0) {
				int firstMatchIndex = matches[0].Index;
				if(firstMatchIndex > 0) {
					yield return format[..firstMatchIndex];
				}
				for(int i = 1; i < matches.Count; i++) {
					var previousMatch = matches[i - 1];
					var currentMatch = matches[i];
					if(i > 0 && previousMatch.Index + previousMatch.Length != currentMatch.Index) {
						yield return format[(previousMatch.Index + previousMatch.Length)..currentMatch.Index];
					}
				}
				var lastMatch = matches[^1];
				if(lastMatch.Index < format.Length - 1) {
					yield return format[(lastMatch.Index + lastMatch.Length)..];
				}
			} else {
				yield return format;
			}
		}

	}

}