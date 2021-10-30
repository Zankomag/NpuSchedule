using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Configs;
using NpuSchedule.Core.Services;

namespace NpuSchedule.Core.Extensions {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection AddNpuScheduleServiceServices(this IServiceCollection services, IConfiguration configuration) {
			services.AddOptions<NpuScheduleOptions>(configuration, NpuScheduleOptions.SectionName);
			services.AddSingleton<INpuScheduleService, NmuNpuScheduleService>();
			return services;
		}

	}

}