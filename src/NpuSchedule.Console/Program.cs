using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Enums;
using NpuSchedule.Core.Extensions;


var host = new HostBuilder()
	.AddConfiguration()
	.ConfigureServices((context, services) => {
		services.AddLogging(logging => logging.AddConsole());
		services.AddNpuScheduleServiceServices(context.Configuration);
	})
	.Build();

var logger = host.Services.GetRequiredService<ILogger<object>>();
logger.LogInformation("Requesting closest day schedule");

var npuScheduleService = host.Services.GetRequiredService<INpuScheduleService>();
var closestDaySchedule = await npuScheduleService.GetDayScheduleAsync(RelativeScheduleDay.Closest);

logger.LogInformation("Schedule retrieved: {Bool}", closestDaySchedule != null);