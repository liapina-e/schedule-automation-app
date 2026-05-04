using System;

namespace schedule_automation_app_client.Services.Dtos;

public record OptimizationItemDto(
    Guid ComponentId,
    string ComponentName,
    double CurrentGrade,
    double RequiredGrade,
    int Priority,
    string Reason);