using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.Entities;

public class OptimizationItem : BaseEntity
{
    public Guid ComponentId { get; private set; }
    public string ComponentName { get; private set; } = string.Empty;
    public double CurrentGrade { get; private set; }
    public double RequiredGrade { get; private set; }
    public int Priority { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private OptimizationItem() { }

    public OptimizationItem(GradeComponent component, double requiredGrade, int priority, string reason)
    {
        if (component == null)
        {
            throw new ArgumentNullException(nameof(component));
        }

        if (requiredGrade < 0 || requiredGrade > 10)
        {
            throw new ArgumentException("Требуемая оценка должна быть от 0 до 10.");
        }

        if (priority <= 0)
        {
            throw new ArgumentException("Приоритет должен быть положительным числом.");
        }

        ComponentId = component.Id;
        ComponentName = component.Name;
        CurrentGrade = component.CurrentGrade.Value;
        RequiredGrade = requiredGrade;
        Priority = priority;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public bool IsImprovementNeeded() => RequiredGrade > CurrentGrade;
}
