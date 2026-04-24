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

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
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