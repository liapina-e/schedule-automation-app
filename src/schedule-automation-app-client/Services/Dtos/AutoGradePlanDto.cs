using System.Collections.Generic;

namespace schedule_automation_app_client.Services.Dtos;

public record AutoGradePlanDto(
    bool IsAchievable,
    string Recommendation,
    List<OptimizationItemDto> Plan,
    List<AutoGradeComponentInfoDto> AutoGradeComponents);