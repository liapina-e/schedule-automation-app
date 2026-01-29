using schedule_automation_app_server.Application.Services;
using schedule_automation_app_server.Infrastructure.Data;
using schedule_automation_app_server.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=schedule.db"));

builder.Services.AddScoped<SubjectRepository>();
builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();

WebApplication app = builder.Build();

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();