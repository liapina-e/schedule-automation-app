using FluentValidation;
using Microsoft.EntityFrameworkCore;
using schedule_automation_app_server.Application.Services.Implementation;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Infrastructure.Data;
using schedule_automation_app_server.Infrastructure.Repositories;
using schedule_automation_app_server.WebAPI.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IStatsService, StatsService>();

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=schedule.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

WebApplication app = builder.Build();

ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

if (allowedOrigins.Length == 0)
{
    logger.LogWarning("CORS: список разрешённых origins пуст — разрешены все origins. Настройте Cors:AllowedOrigins в appsettings.json для продакшена.");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowClient");
app.UseAuthorization();
app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();