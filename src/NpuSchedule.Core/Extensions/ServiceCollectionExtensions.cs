using AngleSharp;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Options;
using NpuSchedule.Core.Services;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

// ReSharper disable UnusedMethodReturnValue.Global

namespace NpuSchedule.Core.Extensions; 

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddNpuScheduleServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddGoogleSheetScheduleServices(configuration);
		return services;
	}

	public static IServiceCollection AddNmuScheduleServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<NmuScheduleOptions>(configuration, NmuScheduleOptions.SectionName);
		services.AddHttpClient<NmuNpuScheduleService>();
		services.AddAngleSharp();
		services.AddSingleton<INpuScheduleService, NmuNpuScheduleService>();
		
		return services;
	}

	public static IServiceCollection AddAngleSharp(this IServiceCollection services) => services.AddSingleton(BrowsingContext.New());

	public static IServiceCollection AddGoogleSheetScheduleServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddHttpClient<GoogleSheetNpuScheduleService>();
		services.AddOptions<GoogleSheetScheduleOptions>(configuration, GoogleSheetScheduleOptions.SectionName);
		services.AddSingleton<SheetsService>(serviceProvider => new SheetsService(new BaseClientService.Initializer {
			ApiKey = serviceProvider.GetRequiredService<IOptions<GoogleSheetScheduleOptions>>().Value.GoogleApiKey,
			ApplicationName = "NPU Schedule Service"
		}));
		services.AddSingleton<INpuScheduleService, GoogleSheetNpuScheduleService>();
		
		return services;
	}

}