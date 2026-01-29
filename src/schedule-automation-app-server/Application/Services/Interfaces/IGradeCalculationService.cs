using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services;

public interface IGradeCalculationService
{
    double CalculateCurrentGrade(Subject subject);
    List<GradeComponent> GetOptimalPlan(Subject subject);
}