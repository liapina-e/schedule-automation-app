using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Mappers;

public static class SubjectMapper
{
    public static Subject ToDomain(CreateSubjectRequest request)
    {
        Subject subject = new Subject(request.Name, request.TargetGrade);

        foreach (ComponentDto dto in request.Components)
        {
            subject.AddComponent(MapComponent(dto));
        }

        return subject;
    }

    public static SubjectResponse ToResponse(Subject subject, OptimizationPlan plan, List<OptimizationPlan>? plansRange = null)
    {
        return new SubjectResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            CurrentGrade = Math.Round(plan.CurrentGrade, 2),
            TargetGrade = subject.TargetGrade,
            IsAchievable = plan.IsAchievable,
            Recommendation = plan.Recommendation,
            OptimalPlan = plan.Items.Select(ToOptimizationItemDto).ToList(),
            PlansRange = plansRange?.Select(p => new GradePlanDto
            {
                TargetGrade = p.TargetGrade,
                IsAchievable = p.IsAchievable,
                Recommendation = p.Recommendation,
                Plan = p.Items.Select(ToOptimizationItemDto).ToList()
            }).ToList() ?? new List<GradePlanDto>()
        };
    }

    public static SubjectListItemResponse ToListItemResponse(Subject subject, double currentGrade)
    {
        return new SubjectListItemResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            TargetGrade = subject.TargetGrade,
            CurrentGrade = Math.Round(currentGrade, 2),
            ComponentCount = subject.Components.Count
        };
    }

    private static GradeComponent MapComponent(ComponentDto dto)
    {
        return new GradeComponent(
            dto.Name,
            dto.Weight,
            dto.Complexity,
            dto.CurrentGrade,
            dto.IsGraded
        )
        {
            IsBlocking = dto.IsBlocking,
            MinimumGrade = dto.MinimumGrade,
            IsAutoGrade = dto.IsAutoGrade,
            AutoGradeMinScore = dto.AutoGradeMinScore
        };
    }

    private static OptimizationItemDto ToOptimizationItemDto(OptimizationItem item)
    {
        return new OptimizationItemDto
        {
            ComponentId = item.ComponentId,
            ComponentName = item.ComponentName,
            CurrentGrade = Math.Round(item.CurrentGrade, 2),
            RequiredGrade = item.RequiredGrade,
            Priority = item.Priority,
            Reason = item.Reason
        };
    }
}