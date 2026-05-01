using schedule_automation_app_server.Domain.Common;
using schedule_automation_app_server.Domain.Exceptions;

namespace schedule_automation_app_server.Domain.Entities;

public class GradeComponent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Complexity { get; set; }
    public double CurrentGrade { get; set; }
    public bool IsBlocking { get; set; }
    public double MinimumGrade { get; set; }
    public bool IsGraded { get; set; }
    public Guid SubjectId { get; set; }

    private GradeComponent() { }

    public GradeComponent(string name, double weight, int complexity, double currentGrade, bool isGraded = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Название компонента не может быть пустым.");
        }

        if (weight < 0 || weight > 100)
        {
            throw new DomainValidationException("Вес должен быть от 0 до 100.");
        }

        if (complexity < 1 || complexity > 10)
        {
            throw new DomainValidationException("Сложность должна быть от 1 до 10.");
        }

        if (currentGrade < 0 || currentGrade > 10)
        {
            throw new DomainValidationException("Оценка должна быть от 0 до 10.");
        }

        Name = name;
        Weight = weight;
        Complexity = complexity;
        CurrentGrade = currentGrade;
        IsGraded = isGraded;
    }

    public double WeightAsDecimal() => Weight / 100.0;

    public bool CanBeImproved() => !IsGraded && CurrentGrade < 10.0 - 1e-6;
}