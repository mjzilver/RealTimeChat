using B4mServer.Data;
using B4mServer.Websockets;
using B4mServer.Websockets.Interfaces;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using B4mServer.Middleware;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace B4mServer;

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

		// Add services
		services.AddSingleton<IMemoryStore, MemoryStore>();
		services.AddSingleton<IWebSocketSender, WebSocketSender>();
		// Command processors
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
