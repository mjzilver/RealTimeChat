namespace RealTimeChatServer;
public static class Program
{
	public static void Main(string[] args)
	{
		CreateHostBuilder(args).Build().Run();
	}

	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(static (webBuilder) =>
			{
				webBuilder.UseStartup<Startup>();
				webBuilder.UseUrls("http://*:5000");
			});
	}
}
