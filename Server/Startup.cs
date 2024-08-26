using B4mServer.Data;
using B4mServer.Websockets;
using B4mServer.Websockets.Interfaces;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.WebSockets;
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
		services.AddScoped<IUserCommandProcessor, UserCommandProcessor>();
		services.AddScoped<IChannelCommandProcessor, ChannelCommandProcessor>();
		services.AddScoped<IMessageCommandProcessor, MessageCommandProcessor>();
		services.AddScoped<IWebSocketCommandProcessor, WebSocketCommandProcessor>();

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

		app.Use(async (context, next) =>
		{
			if (context.Request.Path == "/ws")
			{
				if (context.WebSockets.IsWebSocketRequest)
				{
					WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
					IWebSocketCommandProcessor commandProcessor = context.RequestServices.GetRequiredService<IWebSocketCommandProcessor>();
					IMemoryStore memoryStore = context.RequestServices.GetRequiredService<IMemoryStore>();
					JsonSerializerOptions options = context.RequestServices.GetRequiredService<JsonSerializerOptions>();

					WebSocketHandler handler = new(webSocket, commandProcessor, memoryStore, options);
					await handler.Handle();
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				}
			}
			else
			{
				await next();
			}
		});
	}
}
