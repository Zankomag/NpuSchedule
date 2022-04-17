using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// ReSharper disable UnusedMethodReturnValue.Global

namespace NpuSchedule.Bot.Extensions; 

public static class TelegramBotClientExtensions {

	private const int maxMessageLength = 4096;

	/// <summary>
	///     Adds retry logic for some cases of unsuccessful messages sent
	/// </summary>
	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	[SuppressMessage("ReSharper", "FunctionComplexityOverflow")]
	public static async Task<IList<Message>> SendTextMessageWithRetryAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = default, IEnumerable<MessageEntity> entities = default,
		bool disableWebPagePreview = default, bool disableNotification = default, int replyToMessageId = default, bool allowSendingWithoutReply = default,
		IReplyMarkup replyMarkup = default, CancellationToken cancellationToken = default) {

		try {
			//Messages with a markup are not checked by length because actual length of message after processing entities by telegram will be much less
			if((parseMode == ParseMode.Default && text.Length <= maxMessageLength) || parseMode != ParseMode.Default) {
				Message message = await client.SendTextMessageAsync(chatId, text, parseMode, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
					cancellationToken);
				List<Message> sentMessages = new List<Message>(1) { message };
				return sentMessages;
			}
			return await client.SendMessageSplitByLimitParts(true, chatId, text, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
				cancellationToken);
		} catch(ApiRequestException ex) when(ex.Message.StartsWith("Bad Request: can't parse entities", StringComparison.Ordinal)) {
			await WaitToPreventSpam();

			//Sending message without markdown in case of broken entities
			parseMode = ParseMode.Default;
			text = String.Concat("Message has broken markdown entities, sending raw text:\n\n", text);
			return await client.SendTextMessageWithRetryAsync(chatId, text, parseMode, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
				cancellationToken);
		} catch(ApiRequestException ex) when(ex.Message.StartsWith("Bad Request: message is too long", StringComparison.Ordinal)) {
			//This is intended to catch an error if message has a markup (parseMode is not default) and didn't fit in max length after being processed by telegram,
			//also for raw messages as sending logic handles message length before sending it, this exception handler will work in case if Telegram changes their message length limit to less than 4096
			await WaitToPreventSpam();
			return await client.SendMessageSplitByLimitParts(true, chatId, text, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
				cancellationToken);
		}
	}

	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	public static async Task<IList<Message>> SendMessageSplitByLimitParts(this ITelegramBotClient client, bool sendWarning, ChatId chatId, string text, IEnumerable<MessageEntity> entities,
		bool disableWebPagePreview, bool disableNotification, int replyToMessageId, bool allowSendingWithoutReply,
		IReplyMarkup replyMarkup, CancellationToken cancellationToken) {

		List<Message> sentMessages = new List<Message>();

		if(sendWarning) {
			text = String.Concat("Message is too long, sending row with several parts:\n\n", text);
		}
		foreach(string textPart in SplitMessageByLimitParts(text)) {
			//Sending message without markdown to prevent broken entities
			var messages = await client.SendTextMessageWithRetryAsync(chatId, textPart, ParseMode.Default, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply,
				replyMarkup,
				cancellationToken);
			sentMessages.AddRange(messages);
			await WaitToPreventSpam();
		}
		return sentMessages;
	}

	/// <summary>
	///     Waits a bit to prevent spamming requests to telegram
	/// </summary>
	private static async Task WaitToPreventSpam() => await Task.Delay(200);

	private static IEnumerable<string> SplitMessageByLimitParts(string message) {
		if(message is null) throw new ArgumentNullException(nameof(message));
		for(int i = 0; i < message.Length; i += maxMessageLength) {
			yield return message.Substring(i, Math.Min(maxMessageLength, message.Length - i));
		}
	}

}