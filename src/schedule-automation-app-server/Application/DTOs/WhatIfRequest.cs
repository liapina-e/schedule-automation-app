namespace schedule_automation_app_server.Application.DTOs;

public class WhatIfRequest
{
    public Guid SubjectId { get; set; }
    public List<WhatIfComponentDto> HypotheticalGrades { get; set; } = new();
}