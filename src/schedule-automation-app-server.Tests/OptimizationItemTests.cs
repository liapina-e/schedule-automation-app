using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class OptimizationPlanTests
{
    [Fact]
    public void OptimizationPlan_Constructor_StoresFields()
    {
        Subject subject = new Subject("Математика", 7);
        List<OptimizationItem> items = new List<OptimizationItem>();

        OptimizationPlan plan = new OptimizationPlan(subject, 7, 4.5, 2.5, true, items, "Рекомендация");

        Assert.Equal(subject.Id, plan.SubjectId);
        Assert.Equal(7, plan.TargetGrade);
        Assert.Equal(4.5, plan.CurrentGrade);
        Assert.Equal(2.5, plan.NecessaryPoints);
        Assert.True(plan.IsAchievable);
        Assert.Empty(plan.Items);
        Assert.Equal("Рекомендация", plan.Recommendation);
    }

    [Fact]
    public void OptimizationPlan_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new OptimizationPlan(null!, 7, 4, 3, true, new List<OptimizationItem>(), "Рек"));
    }

    [Fact]
    public void OptimizationPlan_NullItems_ThrowsArgumentNullException()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Throws<ArgumentNullException>(() =>
            new OptimizationPlan(subject, 7, 4, 3, true, null!, "Рек"));
    }

    [Fact]
    public void OptimizationPlan_NullRecommendation_ThrowsArgumentNullException()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Throws<ArgumentNullException>(() =>
            new OptimizationPlan(subject, 7, 4, 3, true, new List<OptimizationItem>(), null!));
    }

    [Fact]
    public void OptimizationPlan_HasNonEmptyId()
    {
        Subject subject = new Subject("Тест", 7);
        OptimizationPlan plan = new OptimizationPlan(subject, 7, 4, 3, true, new List<OptimizationItem>(), "Рек");
        Assert.NotEqual(Guid.Empty, plan.Id);
    }

    [Fact]
    public void OptimizationPlan_CreatedAtIsSet()
    {
        Subject subject = new Subject("Тест", 7);
        OptimizationPlan plan = new OptimizationPlan(subject, 7, 4, 3, true, new List<OptimizationItem>(), "Рек");
        Assert.True(plan.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }
}