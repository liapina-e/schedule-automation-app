namespace schedule_automation_app_server.Application.DTOs;

public class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public int TargetGrade { get; set; }
    public List<ComponentDto> Components { get; set; } = new();
}