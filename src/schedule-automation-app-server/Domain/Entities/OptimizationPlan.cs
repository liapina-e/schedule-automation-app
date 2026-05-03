using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.Entities;

public class OptimizationPlan : BaseEntity
{
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public int TargetGrade { get; private set; }
    public double CurrentGrade { get; private set; }
    public double NecessaryPoints { get; private set; }
    public bool IsAchievable { get; private set; }
    public List<OptimizationItem> Items { get; private set; } = new();
    public string Recommendation { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private OptimizationPlan() { }

    public OptimizationPlan(
        Subject subject,
        int targetGrade,
        double currentGrade,
        double necessaryPoints,
        bool isAchievable,
        List<OptimizationItem> items,
        string recommendation)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        SubjectId = subject.Id;
        TargetGrade = targetGrade;
        CurrentGrade = currentGrade;
        NecessaryPoints = necessaryPoints;
        IsAchievable = isAchievable;
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Recommendation = recommendation ?? throw new ArgumentNullException(nameof(recommendation));
        CreatedAt = DateTime.UtcNow;
    }
}