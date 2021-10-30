using Microsoft.AspNetCore.Hosting;

namespace NpuSchedule.Bot {



	public class LambdaEntryPoint : APIGatewayProxyFunction {

		protected override void Init(IWebHostBuilder builder) 
			=> builder.UseStartup<Startup>();

	}



}