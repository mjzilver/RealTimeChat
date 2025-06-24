using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Middleware;
using RealTimeChatServer.Websockets;
using RealTimeChatServer.Websockets.Interfaces;

namespace RealTimeChatServer;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlite("Data Source=app.db");
		}, ServiceLifetime.Singleton);

		services.AddSingleton(new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		});

		services.AddSingleton<IMemoryStore, MemoryStore>();
		services.AddSingleton<IWebSocketSender, WebSocketSender>();

		services.AddSingleton<IUserCommandProcessor, UserCommandProcessor>();
		services.AddSingleton<IChannelCommandProcessor, ChannelCommandProcessor>();
		services.AddSingleton<IMessageCommandProcessor, MessageCommandProcessor>();
		services.AddSingleton<IWebSocketCommandProcessor, WebSocketCommandProcessor>();

		services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.FromMinutes(2);
		});
		services.AddControllers();
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		dbContext.Database.Migrate();

		app.UseWebSockets();
		app.UseRouting();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		app.UseMiddleware<CustomWebSocketMiddleware>();
	}
}
