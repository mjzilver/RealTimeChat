using B4mServer.Data;
using B4mServer.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(builder =>
	{
		builder.AllowAnyHeader()
			.AllowAnyMethod()
			.SetIsOriginAllowed(_ => true)
			.AllowCredentials();
	});
});

builder.Services.AddDbContext<ChatDbContext>(options =>
{
	options.UseSqlite("Data Source=b4m.db");
});

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

// if development
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Urls.Add("http://localhost:5000");

app.Run();
