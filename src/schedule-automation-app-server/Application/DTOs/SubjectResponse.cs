using System;
using System.Collections.Generic;

namespace schedule_automation_app_server.Application.DTOs;

public class SubjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double CurrentGrade { get; set; }
    public int TargetGrade { get; set; }
    public bool IsAchievable { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<OptimizationItemDto> OptimalPlan { get; set; } = new();
    public List<GradePlanDto> PlansRange { get; set; } = new();
    public bool HasAutoGradeOption { get; set; }
    public AutoGradePlanDto? PlanWithAuto { get; set; }
    public List<ComponentDto> Components { get; set; } = new();
}