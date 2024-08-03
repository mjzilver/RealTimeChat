using B4mServer.Data;
using B4mServer.Websockets;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace B4mServer;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options => {
			options.UseSqlite("Data Source=app.db");
		}, ServiceLifetime.Singleton);

		services.AddSingleton(new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		});

		services.AddSingleton<MemoryStore>();
		services.AddSingleton<WebSocketSender>();
		services.AddScoped<UserCommandProcessor>();
		services.AddScoped<ChannelCommandProcessor>();
		services.AddScoped<MessageCommandProcessor>();
		services.AddScoped<WebSocketCommandProcessor>();

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
					var webSocket = await context.WebSockets.AcceptWebSocketAsync();
					var commandProcessor = context.RequestServices.GetRequiredService<WebSocketCommandProcessor>();
					var memoryStore = context.RequestServices.GetRequiredService<MemoryStore>();
					var options = context.RequestServices.GetRequiredService<JsonSerializerOptions>();
					var handler = new WebSocketHandler(context, webSocket, commandProcessor, 
						memoryStore, options);
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
