using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace NpuSchedule.Bot.Abstractions {

	//TODO add unit and integration tests
	public interface ITelegramBotService : IUpdateHandler {

		Task HandleMessageAsync(Message message);

		Task HandleInlineQueryAsync(InlineQuery inlineQuery);

		Task HandleUpdateAsync(Update update);

		bool IsTokenCorrect(string token);

	}

}