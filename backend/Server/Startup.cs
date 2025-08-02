using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;

using RealTimeChatServer.Data;
using RealTimeChatServer.Middleware;
using RealTimeChatServer.Websockets;
using RealTimeChatServer.Websockets.Dispatcher;
using RealTimeChatServer.Websockets.Interfaces;
using RealTimeChatServer.Websockets.Processors;

namespace RealTimeChatServer;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlite("Data Source=app.db");
		}, ServiceLifetime.Scoped);

		services.AddSingleton(new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		});

		services.AddSingleton<IMemoryStore, MemoryStore>();
		services.AddSingleton<IWebSocketSender, WebSocketSender>();

		services.AddScoped<IUserCommandProcessor, UserCommandProcessor>();
		services.AddScoped<IChannelCommandProcessor, ChannelCommandProcessor>();
		services.AddScoped<IMessageCommandProcessor, MessageCommandProcessor>();

        services.AddScoped<IWebSocketCommandDispatcher, WebSocketCommandDispatcher>();

		var commandTypes = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => typeof(IWebSocketCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

		foreach (var type in commandTypes)
		{
			services.AddScoped(type);
		}

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

		app.UseMiddleware<ChatWebSocketMiddleware>();
	}
}
