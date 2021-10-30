using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NpuSchedule.Bot.Services;
using NpuSchedule.Common.Extensions;

namespace NpuSchedule.Bot {

	public class LocalEntryPoint {

		private static async Task Main() {
			var host = GetHost();
			//var host = GetWebHost();
			
			await host.RunAsync();
		}

		private static IHost GetHost()
			=> new HostBuilder()
				.AddConfiguration()
				.UseStartup<Startup>()
				.ConfigureServices(services => services.AddHostedService<TelegramBotLocalRunner>())
				.Build();

		private static IHost GetWebHost()
			=> new HostBuilder()
				.AddConfiguration()
				.ConfigureWebHost(x =>
					x.UseKestrel()
						.UseUrls("https://*:5930")
						.UseStartup<Startup>())
				.Build();

	}

}