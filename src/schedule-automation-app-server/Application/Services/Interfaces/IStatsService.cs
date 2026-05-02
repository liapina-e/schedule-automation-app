using schedule_automation_app_server.Application.DTOs;

namespace schedule_automation_app_server.Application.Services.Interfaces;

public interface IStatsService
{
    Task<StatsResponse> GetStatsAsync();
}