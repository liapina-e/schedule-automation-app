using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services.Dtos;

namespace schedule_automation_app_client.Services;

public interface IApiService
{
    Task<ObservableCollection<Subject>> LoadSubjectsAsync();
    Task<(PlanResponseDto? Result, ApiError Error)> CreateSubjectAsync(Subject subject);
    Task<(PlanResponseDto? Result, ApiError Error)> UpdateSubjectAsync(Subject subject);
    Task<bool> DeleteSubjectAsync(Guid id);
    Task<PlanResponseDto?> CalculatePlanAsync(Guid id);
}