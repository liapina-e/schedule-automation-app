namespace schedule_automation_app_server.Application.DTOs;

public class WhatIfComponentDto
{
    public string ComponentName { get; set; } = string.Empty;
    public double HypotheticalGrade { get; set; }
}