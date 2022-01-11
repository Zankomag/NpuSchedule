using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NpuSchedule.Common.Enums;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Extensions;

Console.OutputEncoding = Encoding.UTF8;

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
(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset) = RelativeScheduleDay.Closest.GetScheduleDateTimeOffsetRange();
var schedule = await npuScheduleService.GetSchedulesAsync(startDateTimeOffset, endDateTimeOffset);

logger.LogInformation("Number of classes on {StartDateTimeOffset} for {GroupName}: {ClassCount}", startDateTimeOffset, schedule.GroupName, schedule.ScheduleDays.Count > 0 ? schedule.ScheduleDays.First().Classes.Count : 0);

Console.ReadLine();