using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Domain.ValueObjects;

namespace schedule_automation_app_server.Application.Mappers;

public static class SubjectMapper
{
    public static Subject ToDomain(CreateSubjectRequest request)
    {
        Subject subject = new Subject(request.Name, request.TargetGrade);

        foreach (ComponentDto dto in request.Components)
        {
            GradeComponent component = new GradeComponent(
                dto.Name,
                new Weight(dto.Weight),
                new Complexity(dto.Complexity),
                new Grade(dto.CurrentGrade)
            )
            {
                IsBlocking = dto.IsBlocking,
                MinimumGrade = dto.MinimumGrade
            };

            subject.AddComponent(component);
        }

        return subject;
    }

    public static SubjectResponse ToResponse(Subject subject, OptimizationPlan plan)
    {
        return new SubjectResponse
        {
            Id = subject.Id,
            Name = subject.Name,
            CurrentGrade = Math.Round(plan.CurrentGrade, 2),
            TargetGrade = subject.TargetGrade,
            IsAchievable = plan.IsAchievable,
            Recommendation = plan.Recommendation,
            OptimalPlan = plan.Items.Select(ToOptimizationItemDto).ToList()
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

    private static OptimizationItemDto ToOptimizationItemDto(OptimizationItem item)
    {
        return new OptimizationItemDto
        {
            ComponentName = item.ComponentName,
            CurrentGrade = Math.Round(item.CurrentGrade, 2),
            RequiredGrade = item.RequiredGrade,
            Priority = item.Priority,
            Reason = item.Reason
        };
    }
}