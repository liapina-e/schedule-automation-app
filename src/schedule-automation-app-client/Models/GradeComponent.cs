using System;

namespace schedule_automation_app_client.Models;

public class GradeComponent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Weight { get; set; }
    public int Complexity { get; set; }
    public double CurrentGrade { get; set; }
    
    public GradeComponent()
    {
        Id = Guid.NewGuid();
        Name = "Новый компонент";
        Weight = 20;
        Complexity = 5;
        CurrentGrade = 0;
    }
}