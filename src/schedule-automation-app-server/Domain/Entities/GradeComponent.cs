using schedule_automation_app_server.Domain.Common;
using schedule_automation_app_server.Domain.ValueObjects;

namespace schedule_automation_app_server.Domain.Entities;

public class GradeComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Weight Weight { get; set; } = null!;
    public Complexity Complexity { get; set; } = null!;
    public Grade CurrentGrade { get; set; } = null!;
    public bool IsBlocking { get; set; }
    public double MinimumGrade { get; set; }

    private GradeComponent() { }

    public GradeComponent(string name, Weight weight, Complexity complexity, Grade currentGrade)
    {
        Name = name;
        Weight = weight;
        Complexity = complexity;
        CurrentGrade = currentGrade;
    }

    public bool IsCompleted() => CurrentGrade.Value > 0;

    public bool IsMinimumSatisfied() => !IsBlocking || CurrentGrade.Value >= MinimumGrade;
}
