using AngleSharp;
using Microsoft.Extensions.DependencyInjection;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Options;
using NpuSchedule.Core.Services;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

// ReSharper disable UnusedMethodReturnValue.Global

namespace NpuSchedule.Core.Extensions; 

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddNpuScheduleServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<NmuScheduleOptions>(configuration, NmuScheduleOptions.SectionName);
		services.AddSingleton<INpuScheduleService, NmuNpuScheduleService>();
		services.AddHttpClient<NmuNpuScheduleService>();
		services.AddAngleSharp();
		return services;
	}

	public static IServiceCollection AddAngleSharp(this IServiceCollection services) => services.AddSingleton(BrowsingContext.New());

}