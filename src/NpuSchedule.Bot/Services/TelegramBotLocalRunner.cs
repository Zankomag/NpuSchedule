using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using Telegram.Bot;

namespace NpuSchedule.Bot.Services {


	/// <summary>
	/// This service should be used to run Telegram Bot on a local machine as it uses Telegram Bot API polling
	/// </summary>
	public class TelegramBotLocalRunner : IHostedService {

		private readonly ITelegramBotService telegramBotService;
		private readonly ILogger<TelegramBotLocalRunner> logger;
		private readonly ITelegramBotClient client;

		private CancellationTokenSource cancellationTokenSource;
		private Task pollingTask;

		public TelegramBotLocalRunner(IOptions<TelegramBotOptions> telegramBotOptions, ITelegramBotService telegramBotService, ILogger<TelegramBotLocalRunner> logger) {
			this.telegramBotService = telegramBotService;
			this.logger = logger;
			TelegramBotOptions options = telegramBotOptions.Value;
			client = new TelegramBotClient(options.Token);
		}

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting telegram polling...");
			cancellationTokenSource = new CancellationTokenSource();
			pollingTask = Task.Run(() => client.ReceiveAsync(telegramBotService, cancellationTokenSource.Token), cancellationToken);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Stopping telegram polling...");
			cancellationTokenSource.Cancel();
			await pollingTask;
		}

	}

}