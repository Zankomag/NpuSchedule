using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NpuSchedule.Bot.Abstractions;
using NpuSchedule.Bot.Options;
using NpuSchedule.Bot.Services;
using NpuSchedule.Common.Extensions;

// ReSharper disable UnusedMethodReturnValue.Global

namespace NpuSchedule.Bot.Extensions {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration configuration) {
			services.AddOptions<TelegramBotOptions>(configuration, TelegramBotOptions.SectionName);
			services.AddSingleton<ITelegramBotService, TelegramBotService>();
			services.AddSingleton<ITelegramBotUi, TelegramBotUi>();
			return services;
		}

	}

}