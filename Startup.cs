using B4mServer.Data;
using B4mServer.Websockets;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace B4mServer;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(options => {
			options.UseSqlite("Data Source=app.db");
		}, ServiceLifetime.Singleton);

		services.AddSingleton<MemoryStore>();

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
					using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
					var webSocketHandler = new WebSocketHandler(context, webSocket, dbContext, 
						context.RequestServices.GetRequiredService<MemoryStore>());
					await webSocketHandler.Handle();
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
