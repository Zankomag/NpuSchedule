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

namespace NpuSchedule.Bot.Extensions {


	public static class TelegramBotClientExtensions {

		/// <summary>
		///     Adds retry logic for some cases of unsuccessful messages sent
		/// </summary>
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public static async Task<Message> SendTextMessageWithRetryAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = default, IEnumerable<MessageEntity> entities = default,
			bool disableWebPagePreview = default, bool disableNotification = default, int replyToMessageId = default, bool allowSendingWithoutReply = default,
			IReplyMarkup replyMarkup = default, CancellationToken cancellationToken = default) {

			Message message;

			try {
				message = await client.SendTextMessageAsync(chatId, text, parseMode, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup, cancellationToken);
			} catch(ApiRequestException ex) when(ex.Message.StartsWith("Bad Request: can't parse entities", StringComparison.Ordinal)) {
				
				//Wait a bit to prevent spamming requests to telegram
				await Task.Delay(200, CancellationToken.None);
				
				//Sending message without markdown in case of broken entities
				parseMode = ParseMode.Default;
				text = String.Concat("Message has broken markdown entities, sending raw text:\n\n", text);
				message = await client.SendTextMessageAsync(chatId, text, parseMode, entities, disableWebPagePreview, disableNotification, replyToMessageId, allowSendingWithoutReply, replyMarkup,
					cancellationToken);
			}

			//TODO add errors with message length handling (catch before sending first)
			
			return message;
		}

	}

}