using System;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services.Dtos;

namespace schedule_automation_app_client.Services;

public interface IApiService
{
    Task<PlanResponseDto?> CalculatePlanAsync(Subject subject);
    Task<bool> IsServerAvailableAsync();
}