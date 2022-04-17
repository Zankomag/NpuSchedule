using System;
using System.Diagnostics;
using NpuSchedule.Common.Abstractions;
using NpuSchedule.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IStartup = NpuSchedule.Common.Abstractions.IStartup;

namespace NpuSchedule.Common.Extensions; 

/// <summary>
///     Extensions to emulate a typical "Startup.cs" pattern for <see cref="IHostBuilder" />
/// </summary>
public static class HostBuilderExtensions {

	/// <summary>
	///     Specify the startup type to be used by the host.
	/// </summary>
	/// <typeparam name="TStartup">
	///     The type containing a constructor with
	///     an <see cref="IConfiguration" /> parameter. The implementation should contain a public
	///     method named ConfigureServices with <see cref="IServiceCollection" /> parameter.
	/// </typeparam>
	/// <param name="hostBuilder">The <see cref="IHostBuilder" /> to initialize with TStartup.</param>
	/// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
	public static IHostBuilder UseStartup<TStartup>(this IHostBuilder hostBuilder) where TStartup : IStartup {
		hostBuilder.ConfigureServices((context, serviceCollection) => {
			IStartup startup = (TStartup)Activator.CreateInstance(typeof(TStartup), context.Configuration)!;
			Debug.Assert(startup != null, nameof(startup) + " is null");
			startup.ConfigureServices(serviceCollection);
		});
			
		return hostBuilder;
	}

	/// <summary>
	/// Sets Hosting Environment
	/// Adds config from appsettings.json and appsettings.{{EnvironmentName}}.json files
	/// Loads User Secrets if Development
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <returns></returns>
	public static IHostBuilder AddConfiguration(this IHostBuilder hostBuilder) {
		hostBuilder.ConfigureAppConfiguration((hostingContext, configurationBuilder) => {
			hostingContext.HostingEnvironment.EnvironmentName = EnvironmentWrapper.GetEnvironmentName();
				
			configurationBuilder.AddJsonFile("appsettings.json", false)
				.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", false);

			if(hostingContext.HostingEnvironment.IsDevelopment() && !String.IsNullOrEmpty(hostingContext.HostingEnvironment.ApplicationName)) {
				configurationBuilder.AddUserSecrets<StartupBase>();
			}
			//This is for reading config from Cloud Providers that don't support appsettings.json 
			configurationBuilder.AddEnvironmentVariables();
		});
		return hostBuilder;
	}
		

}