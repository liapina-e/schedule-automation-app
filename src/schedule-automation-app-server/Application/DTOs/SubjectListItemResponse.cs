namespace schedule_automation_app_server.Application.DTOs;

public class SubjectListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TargetGrade { get; set; }
    public double CurrentGrade { get; set; }
    public int ComponentCount { get; set; }
}