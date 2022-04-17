using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NpuSchedule.Common.Abstractions; 

public abstract class StartupBase : IStartup {

	protected IConfiguration Configuration { get; }

	protected StartupBase(IConfiguration configuration) => Configuration = configuration;

	/// <inheritdoc />
	public abstract void ConfigureServices(IServiceCollection services);

}