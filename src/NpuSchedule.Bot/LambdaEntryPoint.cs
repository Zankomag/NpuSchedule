using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;

// ReSharper disable UnusedType.Global

namespace NpuSchedule.Bot; 

public class LambdaEntryPoint : APIGatewayProxyFunction {

	protected override void Init(IWebHostBuilder builder) 
		=> builder.UseStartup<Startup>();

}