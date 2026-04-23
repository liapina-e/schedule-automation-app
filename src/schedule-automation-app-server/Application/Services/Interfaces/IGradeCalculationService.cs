using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Interfaces;

public interface IGradeCalculationService
{
    double CalculateCurrentGrade(Subject subject);
    OptimizationPlan CalculateOptimizationPlan(Subject subject);
}
