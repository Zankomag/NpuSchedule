using Microsoft.Extensions.DependencyInjection;

namespace NpuSchedule.Common.Abstractions; 

public interface IStartup {

	void ConfigureServices(IServiceCollection services);

}