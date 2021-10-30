using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Configs;
using NpuSchedule.Bot.Services;

namespace NpuSchedule.Bot.Extensions {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration configuration) {
			services.AddOptions<TelegramBotOptions>(configuration, TelegramBotOptions.SectionName);
			services.AddSingleton<ITelegramBotService, TelegramBotService>();
			return services;
		}

	}

}