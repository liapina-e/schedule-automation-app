using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services;

public class GradeCalculationService : IGradeCalculationService
{
    public double CalculateCurrentGrade(Subject subject)
    {
        return subject.CalculateCurrentGrade();
    }

    public List<GradeComponent> GetOptimalPlan(Subject subject)
    {
        return subject.Components
            .Where(c => c.CurrentGrade.Value < 10)
            .OrderByDescending(c => c.GetEfficiency())
            .ToList();
    }
}