namespace schedule_automation_app_server.Application.DTOs;

public class OptimizationItemDto
{
    public Guid ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public double CurrentGrade { get; set; }
    public double RequiredGrade { get; set; }
    public int Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
}