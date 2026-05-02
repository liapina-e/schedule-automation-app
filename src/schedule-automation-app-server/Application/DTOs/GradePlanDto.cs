namespace schedule_automation_app_server.Application.DTOs;

public class GradePlanDto
{
    public int TargetGrade { get; set; }
    public bool IsAchievable { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<OptimizationItemDto> Plan { get; set; } = new();
}