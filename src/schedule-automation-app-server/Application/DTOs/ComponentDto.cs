namespace schedule_automation_app_server.Application.DTOs;

public class ComponentDto
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Complexity { get; set; }
    public double CurrentGrade { get; set; }
    public bool IsBlocking { get; set; }
    public double MinimumGrade { get; set; }
}