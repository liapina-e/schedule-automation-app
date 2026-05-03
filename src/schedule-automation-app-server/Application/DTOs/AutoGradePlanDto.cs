namespace schedule_automation_app_server.Application.DTOs;

public class AutoGradePlanDto
{
    public bool IsAchievable { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<OptimizationItemDto> Plan { get; set; } = new();
    public List<AutoGradeComponentInfoDto> AutoGradeComponents { get; set; } = new();
}