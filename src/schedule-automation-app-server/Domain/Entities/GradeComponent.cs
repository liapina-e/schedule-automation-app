using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.Entities;

public class GradeComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Complexity { get; set; }
    public double CurrentGrade { get; set; }
    public bool IsBlocking { get; set; }
    public double MinimumGrade { get; set; }
    public Guid SubjectId { get; set; }

    private GradeComponent() { }

    public GradeComponent(string name, double weight, int complexity, double currentGrade)
    {
        Name = name;
        Weight = weight;
        Complexity = complexity;
        CurrentGrade = currentGrade;
    }

    public double WeightAsDecimal() => Weight / 100.0;
}