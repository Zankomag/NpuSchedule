using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NpuSchedule.Common.Extensions;
using NpuSchedule.Core.Abstractions;
using NpuSchedule.Core.Enums;
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
var rawHtml = await npuScheduleService.GetRawHtmlScheduleResponse(DateTime.Now.AddDays(1), DateTime.Now.AddDays(2));

logger.LogInformation(rawHtml);

Console.ReadLine();