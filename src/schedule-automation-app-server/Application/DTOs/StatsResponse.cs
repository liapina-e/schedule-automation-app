namespace schedule_automation_app_server.Application.DTOs;

public class StatsResponse
{
    public int TotalSubjects { get; set; }
    public double AverageTargetGrade { get; set; }
    public int MostCommonComplexity { get; set; }
    public int TotalComponents { get; set; }
    public double AverageComponentsPerSubject { get; set; }
    public int AchievableSubjects { get; set; }
}