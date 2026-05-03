namespace schedule_automation_app_server.Application.DTOs;

public class AutoGradeComponentInfoDto
{
    public string ComponentName { get; set; } = string.Empty;
    public double CurrentGrade { get; set; }
    public double RequiredMinScore { get; set; }
    public bool IsAlreadyAchieved { get; set; }
}