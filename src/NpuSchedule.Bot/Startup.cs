using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NpuSchedule.Bot.Extensions;
using NpuSchedule.Core.Extensions;

namespace NpuSchedule.Bot {

	public class Startup : NpuSchedule.Common.Abstractions.StartupBase {

		public Startup(IConfiguration configuration) : base(configuration) { }

		public override void ConfigureServices(IServiceCollection services) {
			services.AddNpuScheduleServiceServices(Configuration);
			services.AddTelegramBotServices(Configuration);
			services.AddLogging(x => x.AddConsole());
			services.AddControllers()
				.AddNewtonsoftJson(options => {
					options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
					options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
				});
			services.AddHealthChecks();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			//app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();
			app.UseHealthChecks("/healthCheck");
			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapGet("/", async context => await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda"));
			});
		}

	}

}