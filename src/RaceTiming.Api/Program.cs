using Microsoft.EntityFrameworkCore;
using RaceTiming.Api.Data;
using RaceTiming.Api.Middleware;
using RaceTiming.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceTiming API",
        Version = "v1",
        Description = "Backend for ingesting checkpoint-arch timing reads and serving race results."
    });
});

builder.Services.AddDbContext<RaceTimingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CheckpointPassRecorder>();
builder.Services.AddScoped<ResultsService>();
builder.Services.AddScoped<INotificationChannel, AuditLogChannel>();
builder.Services.AddScoped<INotificationChannel, LeaderboardCacheChannel>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceTimingDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
