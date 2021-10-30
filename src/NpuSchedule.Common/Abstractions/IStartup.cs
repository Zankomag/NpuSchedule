using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NpuSchedule.Common.Abstractions {

	public interface IStartup {
		public IConfiguration Configuration { get; }
		
		void ConfigureServices(IServiceCollection services);

	}

}