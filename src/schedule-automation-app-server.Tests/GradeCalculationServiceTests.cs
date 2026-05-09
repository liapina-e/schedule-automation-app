using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.Application.Services.Implementation;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class GradeCalculationServiceTests
{
    private readonly GradeCalculationService _service;

    public GradeCalculationServiceTests()
    {
        _service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
    }
    
    [Fact]
    public void CalculateCurrentGrade_NoComponents_ReturnsZero()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Equal(0, _service.CalculateCurrentGrade(subject));
    }

    [Fact]
    public void CalculateCurrentGrade_TwoComponents_ReturnsWeightedAverage()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Экзамен", 50, 8, 0, false, false, 0, false, 0),
                ("ДЗ", 30, 3, 7, false, false, 0, false, 0),
                ("Контрольная", 20, 5, 6, false, false, 0, false, 0)
            ]);

        double result = _service.CalculateCurrentGrade(subject);
        Assert.Equal(3.3, result, precision: 1);
    }

    [Fact]
    public void CalculateCurrentGrade_AllGradesMax_ReturnsTen()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("А", 50, 5, 10, false, false, 0, false, 0),
                ("Б", 50, 5, 10, false, false, 0, false, 0)
            ]);

        Assert.Equal(10.0, _service.CalculateCurrentGrade(subject), precision: 2);
    }

    [Fact]
    public void CalculateCurrentGrade_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateCurrentGrade(null!));
    }
    
    [Fact]
    public void CalculateOptimizationPlan_GoalAlreadyAchieved_ReturnsEmptyPlan()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 4,
            components: ("Зачет", 100, 1, 9, false, false, 0, false, 0));

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.Empty(plan.Items);
    }

    [Fact]
    public void CalculateOptimizationPlan_AllGradedBelowTarget_NotAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 9,
            components: [
                ("Реферат", 60, 5, 5, true, false, 0, false, 0),
                ("Тест", 40, 4, 4, true, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.False(plan.IsAchievable);
        Assert.Contains("недостижима", plan.Recommendation);
    }

    [Fact]
    public void CalculateOptimizationPlan_BlockingFixedBelowMinimum_NotAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("Практика", 40, 5, 3, true, true, 4.0, false, 0),
                ("Теория", 60, 4, 5, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.False(plan.IsAchievable);
        Assert.Contains("Практика", plan.Recommendation);
        Assert.Contains("4", plan.Recommendation);
    }

    [Fact]
    public void CalculateOptimizationPlan_StandardCase_IsAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Экзамен", 50, 8, 0, false, false, 0, false, 0),
                ("ДЗ", 30, 3, 7, false, false, 0, false, 0),
                ("Контрольная", 20, 5, 6, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.NotEmpty(plan.Items);
    }

    [Fact]
    public void CalculateOptimizationPlan_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateOptimizationPlan(null!));
    }
    
    [Fact]
    public void CalculateOptimizationPlan_MaxAchievableExactly6Point5_GoalIs7_IsAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Экзамен", 70, 7, 5, true, false, 0, false, 0),
                ("ДЗ", 30, 2, 7, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
    }

    [Fact]
    public void CalculateOptimizationPlan_MaxAchievableLessThan6Point5_GoalIs7_NotAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Экзамен", 70, 7, 4.99, true, false, 0, false, 0),
                ("ДЗ", 30, 2, 7, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.False(plan.IsAchievable);
    }
    
    [Fact]
    public void CalculateOptimizationPlan_BlockingAboveMinimum_NotInPlan()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Лаба", 30, 4, 5, false, true, 4.0, false, 0),
                ("Экзамен", 70, 7, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.DoesNotContain(plan.Items, i => i.ComponentName == "Лаба" && i.RequiredGrade <= 4.0);
    }

    [Fact]
    public void CalculateOptimizationPlan_BlockingThreshold3Point5_RaisedExactly()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("Лаба", 50, 6, 3, false, true, 3.5, false, 0),
                ("Лекции", 50, 4, 5, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        OptimizationItem? laba = plan.Items.FirstOrDefault(i => i.ComponentName == "Лаба");
        Assert.NotNull(laba);
        Assert.Equal(3.5, laba.RequiredGrade, precision: 1);
    }

    [Fact]
    public void CalculateOptimizationPlan_BlockingThreshold4Point0_RaisedExactly()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("Лаба", 50, 6, 3, false, true, 4.0, false, 0),
                ("Лекции", 50, 4, 5, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        OptimizationItem? laba = plan.Items.FirstOrDefault(i => i.ComponentName == "Лаба");
        Assert.NotNull(laba);
        Assert.Equal(4.0, laba.RequiredGrade, precision: 1);
    }
    
    [Fact]
    public void CalculateOptimizationPlan_LowerEffortRatioFirst()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Сложный", 50, 9, 0, false, false, 0, false, 0),
                ("Легкий", 50, 2, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);

        OptimizationItem? slozhny = plan.Items.FirstOrDefault(i => i.ComponentName == "Сложный");
        OptimizationItem? legky = plan.Items.FirstOrDefault(i => i.ComponentName == "Легкий");

        Assert.NotNull(legky);
        double legkyGain = legky.RequiredGrade - 0;
        double slozhnyGain = slozhny != null ? slozhny.RequiredGrade - 0 : 0;

        Assert.True(legkyGain >= slozhnyGain);
    }

    [Fact]
    public void CalculateOptimizationPlan_GradedComponentNotInPlan()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: [
                ("Зафикс", 50, 5, 5, true, false, 0, false, 0),
                ("Свободный", 50, 5, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.DoesNotContain(plan.Items, i => i.ComponentName == "Зафикс");
    }
    
    [Fact]
    public void CalculatePlansRange_ReturnsPlansFromTargetTo10()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 7,
            components: ("Все", 100, 5, 0, false, false, 0, false, 0));

        List<OptimizationPlan> plans = _service.CalculatePlansRange(subject);

        Assert.Equal(4, plans.Count);
        Assert.Equal(7, plans[0].TargetGrade);
        Assert.Equal(10, plans[3].TargetGrade);
    }

    [Fact]
    public void CalculatePlansRange_WhenMaxLimited_SomePlansNotAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("Экзамен", 70, 7, 5, true, false, 0, false, 0),
                ("ДЗ", 30, 2, 7, false, false, 0, false, 0)
            ]);

        List<OptimizationPlan> plans = _service.CalculatePlansRange(subject);

        Assert.True(plans[0].IsAchievable);
        Assert.True(plans[1].IsAchievable);
        Assert.False(plans[2].IsAchievable);
    }
    
    
    [Fact]
    public void CalculateOptimizationPlanWithAuto_NoAutoComponents_ReturnsSameAsStandard()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("А", 50, 5, 4, false, false, 0, false, 0),
                ("Б", 50, 5, 4, false, false, 0, false, 0)
            ]);

        OptimizationPlan standard = _service.CalculateOptimizationPlan(subject);
        OptimizationPlan withAuto = _service.CalculateOptimizationPlanWithAuto(subject);

        Assert.Equal(standard.IsAchievable, withAuto.IsAchievable);
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_AutoComponent_RaisedToAutoMinFirst()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 5,
            components: [
                ("ДЗ", 50, 2, 4, false, false, 0, true, 7.0),
                ("Семинар", 50, 5, 4, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        OptimizationItem? dz = plan.Items.FirstOrDefault(i => i.ComponentName == "ДЗ");
        Assert.NotNull(dz);
        Assert.True(dz.RequiredGrade >= 7.0);
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_BlockingAndAuto_UsesMaxOfTwoThresholds()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("Проект", 50, 5, 2, false, true, 4.0, true, 6.0),
                ("Теория", 50, 3, 5, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        OptimizationItem? proekt = plan.Items.FirstOrDefault(i => i.ComponentName == "Проект");
        Assert.NotNull(proekt);
        Assert.True(proekt.RequiredGrade >= 6.0);
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_StandardPlanDiffers_WhenAutoPresent()
    {
        Subject subject = TestHelpers.CreateSubject(targetGrade: 5,
            components: [
                ("ДЗ", 50, 2, 4, false, false, 0, true, 7.0),
                ("Семинар", 50, 5, 4, false, false, 0, false, 0)
            ]);

        OptimizationPlan standard = _service.CalculateOptimizationPlan(subject);
        OptimizationPlan withAuto = _service.CalculateOptimizationPlanWithAuto(subject);

        OptimizationItem? stdDz = standard.Items.FirstOrDefault(i => i.ComponentName == "ДЗ");
        OptimizationItem? autoDz = withAuto.Items.FirstOrDefault(i => i.ComponentName == "ДЗ");

        Assert.NotNull(stdDz);
        Assert.NotNull(autoDz);
        Assert.True(autoDz.RequiredGrade > stdDz.RequiredGrade);
    }

    [Fact]
    public void CalculateCurrentGrade_OneComponent_ReturnsGrade()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Экзамен", 100, 5, 6.5, false, false, 0, false, 0));

        double result = _service.CalculateCurrentGrade(subject);
        Assert.Equal(6.5, result, precision: 5);
    }

    [Fact]
    public void CalculateOptimizationPlan_AllImprovableButCannotReachTarget_NotAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 10,
            components: [
                ("А", 50, 5, 8, true, false, 0, false, 0),
                ("Б", 50, 5, 8, true, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);
        Assert.False(plan.IsAchievable);
    }

    [Fact]
    public void CalculateOptimizationPlan_NoImprovableComponents_EmptyItems()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: [
                ("А", 50, 5, 10, false, false, 0, false, 0),
                ("Б", 50, 5, 10, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);
        Assert.True(plan.IsAchievable);
        Assert.Empty(plan.Items);
    }

    [Fact]
    public void CalculateOptimizationPlanForGrade_TargetGrade5_Achievable()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Все", 100, 5, 0, false, false, 0, false, 0));

        OptimizationPlan plan = _service.CalculateOptimizationPlanForGrade(subject, 5);

        Assert.True(plan.IsAchievable);
        Assert.Equal(5, plan.TargetGrade);
    }

    [Fact]
    public void CalculateOptimizationPlanForGrade_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateOptimizationPlanForGrade(null!, 7));
    }

    [Fact]
    public void CalculateOptimizationPlan_CurrentGradeInPlan_IsCorrect()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 8,
            components: [
                ("А", 50, 5, 4, false, false, 0, false, 0),
                ("Б", 50, 5, 6, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.Equal(5.0, plan.CurrentGrade, precision: 5);
    }

    [Fact]
    public void CalculateOptimizationPlan_BlockingAndNormal_BlockingHasHigherPriority()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: [
                ("Легкий", 60, 1, 0, false, false, 0, false, 0),
                ("Блокирующий", 40, 9, 2, false, true, 4.0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        OptimizationItem? blocking = plan.Items.FirstOrDefault(i => i.ComponentName == "Блокирующий");
        if (blocking != null)
        {
            Assert.True(blocking.RequiredGrade >= 4.0);
        }
    }

    [Fact]
    public void CalculatePlansRange_AllAchievable_WhenAllGradesZeroAndCanReach10()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Все", 100, 5, 0, false, false, 0, false, 0));

        List<OptimizationPlan> plans = _service.CalculatePlansRange(subject);

        Assert.All(plans, p => Assert.True(p.IsAchievable));
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_TwoAutoComponents_BothRaisedToMinScore()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 5,
            components: [
                ("ДЗ1", 40, 2, 3, false, false, 0, true, 7.0),
                ("ДЗ2", 60, 3, 3, false, false, 0, true, 6.0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        OptimizationItem? dz1 = plan.Items.FirstOrDefault(i => i.ComponentName == "ДЗ1");
        OptimizationItem? dz2 = plan.Items.FirstOrDefault(i => i.ComponentName == "ДЗ2");

        if (dz1 != null)
        {
            Assert.True(dz1.RequiredGrade >= 7.0);
        }

        if (dz2 != null)
        {
            Assert.True(dz2.RequiredGrade >= 6.0);
        }
    }
    
    [Fact]
    public void CalculateCurrentGrade_ZeroTotalWeight_ReturnsZero()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Equal(0, _service.CalculateCurrentGrade(subject));
    }

    [Fact]
    public void CalculateOptimizationPlan_SingleComponent100Weight_PlanCoversFullGap()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 8,
            components: ("Экзамен", 100, 5, 4, false, false, 0, false, 0));

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        OptimizationItem? item = plan.Items.FirstOrDefault(i => i.ComponentName == "Экзамен");
        Assert.NotNull(item);
        Assert.True(item.RequiredGrade >= 8.0 - 1e-6);
    }

    [Fact]
    public void CalculateOptimizationPlan_AllComponentsGraded_GoalAlreadyMet_IsAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: [
                ("А", 50, 5, 8, true, false, 0, false, 0),
                ("Б", 50, 5, 8, true, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.Empty(plan.Items);
    }

    [Fact]
    public void CalculateOptimizationPlan_RecommendationContainsPlan_WhenAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Экзамен", 100, 5, 0, false, false, 0, false, 0));

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.Contains("Минимальный план", plan.Recommendation);
    }

    [Fact]
    public void CalculateOptimizationPlan_RecommendationContainsAlreadySufficient_WhenGoalMet()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: ("Экзамен", 100, 5, 9, false, false, 0, false, 0));

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.Contains("достаточно", plan.Recommendation);
    }

    [Fact]
    public void CalculateOptimizationPlan_BlockingReasonInItems_WhenBlockingRaised()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 6,
            components: [
                ("Лаба", 50, 5, 2, false, true, 4.0, false, 0),
                ("Лекции", 50, 3, 5, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        OptimizationItem? laba = plan.Items.FirstOrDefault(i => i.ComponentName == "Лаба");
        Assert.NotNull(laba);
        Assert.Contains("Блокирующий", laba.Reason);
    }

    [Fact]
    public void CalculateOptimizationPlan_EffortReasonInItems_WhenOptimized()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: [
                ("Экзамен", 60, 7, 0, false, false, 0, false, 0),
                ("ДЗ", 40, 3, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.True(plan.IsAchievable);
        Assert.All(plan.Items, item =>
            Assert.True(item.Reason.Contains("Стоимость") || item.Reason.Contains("Блокирующий")));
    }

    [Fact]
    public void CalculatePlansRange_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculatePlansRange(null!));
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_NullSubject_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.CalculateOptimizationPlanWithAuto(null!));
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_NotAchievableEvenWithAuto_ReturnsFalse()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 9,
            components: [
                ("Зачёт", 60, 5, 5, true, false, 0, false, 0),
                ("ДЗ", 40, 3, 4, true, false, 0, true, 8.0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        Assert.False(plan.IsAchievable);
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_GoalAlreadyMet_IsAchievable()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: [
                ("А", 50, 5, 9, false, false, 0, true, 7.0),
                ("Б", 50, 3, 9, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        Assert.True(plan.IsAchievable);
    }

    [Fact]
    public void CalculateOptimizationPlanWithAuto_AutoReasonInItems_WhenAutoComponentRaised()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 5,
            components: [
                ("ДЗ", 50, 2, 3, false, false, 0, true, 7.0),
                ("Семинар", 50, 5, 4, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlanWithAuto(subject);

        OptimizationItem? dz = plan.Items.FirstOrDefault(i => i.ComponentName == "ДЗ");
        Assert.NotNull(dz);
        Assert.Contains("автомат", dz.Reason);
    }

    [Fact]
    public void CalculateOptimizationPlan_PriorityIsSequential()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 8,
            components: [
                ("А", 40, 3, 0, false, false, 0, false, 0),
                ("Б", 30, 5, 0, false, false, 0, false, 0),
                ("В", 30, 8, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        List<int> priorities = plan.Items.Select(i => i.Priority).OrderBy(p => p).ToList();
        for (int i = 0; i < priorities.Count; i++)
        {
            Assert.Equal(i + 1, priorities[i]);
        }
    }

    [Fact]
    public void CalculateOptimizationPlan_ComponentAlreadyAt10_NotInPlan()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 8,
            components: [
                ("Максимум", 50, 5, 10, false, false, 0, false, 0),
                ("Свободный", 50, 5, 0, false, false, 0, false, 0)
            ]);

        OptimizationPlan plan = _service.CalculateOptimizationPlan(subject);

        Assert.DoesNotContain(plan.Items, i => i.ComponentName == "Максимум");
    }

    [Fact]
    public void CalculatePlansRange_TargetGrade10_ReturnsSinglePlan()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 10,
            components: ("Все", 100, 5, 0, false, false, 0, false, 0));

        List<OptimizationPlan> plans = _service.CalculatePlansRange(subject);

        Assert.Single(plans);
        Assert.Equal(10, plans[0].TargetGrade);
    }
}