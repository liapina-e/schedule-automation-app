namespace schedule_automation_app_server.Application.DTOs;

public class WhatIfResponse
{
    public double CurrentGrade { get; set; }
    public double HypotheticalGrade { get; set; }
    public double TargetGrade { get; set; }
    public bool WouldAchieveTarget { get; set; }
    public double PointsRemaining { get; set; }
    public List<WhatIfComponentResultDto> Components { get; set; } = new();
}