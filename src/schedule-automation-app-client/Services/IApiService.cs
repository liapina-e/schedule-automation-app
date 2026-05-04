using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services.Dtos;

namespace schedule_automation_app_client.Services;

public interface IApiService
{
    Task<ObservableCollection<Subject>> LoadSubjectsAsync();
    Task<PlanResponseDto?> CreateSubjectAsync(Subject subject);
    Task<PlanResponseDto?> UpdateSubjectAsync(Subject subject);
    Task<bool> DeleteSubjectAsync(Guid id);
    Task<PlanResponseDto?> CalculatePlanAsync(Guid id);
}