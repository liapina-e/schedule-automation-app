using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Implementation;

public class StatsService : IStatsService
{
    private readonly ISubjectRepository _repository;
    private readonly IGradeCalculationService _calculationService;

    public StatsService(ISubjectRepository repository, IGradeCalculationService calculationService)
    {
        _repository = repository;
        _calculationService = calculationService;
    }

    public async Task<StatsResponse> GetStatsAsync()
    {
        List<Subject> subjects = await _repository.GetAllAsync();

        if (subjects.Count == 0)
        {
            return new StatsResponse();
        }

        List<GradeComponent> allComponents = subjects
            .SelectMany(s => s.Components)
            .ToList();

        int mostCommonComplexity = allComponents.Count > 0
            ? allComponents
                .GroupBy(c => c.Complexity)
                .OrderByDescending(g => g.Count())
                .First().Key
            : 0;

        int achievableCount = subjects.Count(s =>
            _calculationService.CalculateOptimizationPlan(s).IsAchievable);

        return new StatsResponse
        {
            TotalSubjects = subjects.Count,
            AverageTargetGrade = Math.Round(subjects.Average(s => s.TargetGrade), 2),
            MostCommonComplexity = mostCommonComplexity,
            TotalComponents = allComponents.Count,
            AverageComponentsPerSubject = Math.Round((double)allComponents.Count / subjects.Count, 1),
            AchievableSubjects = achievableCount
        };
    }
}