namespace schedule_automation_app_server.Application.DTOs;

public class SubjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double CurrentGrade { get; set; }
    public int TargetGrade { get; set; }
    public List<ComponentDto> OptimalPlan { get; set; } = new();
}