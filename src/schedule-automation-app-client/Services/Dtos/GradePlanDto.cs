using System.Collections.Generic;

namespace schedule_automation_app_client.Services.Dtos;

public record GradePlanDto(int TargetGrade,
    bool IsAchievable,
    string Recommendation,
    List<OptimizationItemDto> Plan);