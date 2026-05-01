namespace schedule_automation_app_server.Application.DTOs;

public class WhatIfComponentResultDto
{
    public string ComponentName { get; set; } = string.Empty;
    public double CurrentGrade { get; set; }
    public double HypotheticalGrade { get; set; }
    public bool IsGraded { get; set; }
    public double WeightedContribution { get; set; }
}