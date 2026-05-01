using System;
using System.Collections.Generic;

namespace schedule_automation_app_client.Services.Dtos;

public record PlanResponseDto(
    Guid Id,
    string Name,
    double CurrentGrade,
    int TargetGrade,
    string Recommendation,
    List<OptimizationItemDto> OptimalPlan);