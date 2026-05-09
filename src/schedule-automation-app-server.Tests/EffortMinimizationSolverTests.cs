using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Mappers;
using schedule_automation_app_server.Application.Services.Implementation;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class EffortMinimizationSolverTests
{
    private readonly EffortMinimizationSolver _solver = new EffortMinimizationSolver();

    private double[] Solve(
        double[] current,
        double[] weights,
        int[] complexities,
        double targetSum,
        bool[]? isBlocking = null,
        double[]? minimumGrades = null)
    {
        isBlocking ??= new bool[current.Length];
        minimumGrades ??= new double[current.Length];
        return _solver.Solve(current, weights, complexities, targetSum, isBlocking, minimumGrades);
    }

    [Fact]
    public void Solve_LowerEffortComponentImprovedFirst()
    {
        double[] current = { 0, 0 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 9, 2 };
        double target = 5.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.True(result[1] > result[0]);
    }

    [Fact]
    public void Solve_BlockingBelowMinimum_RaisedFirst()
    {
        double[] current = { 2, 5 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 3 };
        double target = 6.0;
        bool[] isBlocking = { true, false };
        double[] minimumGrades = { 4.0, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.True(result[0] >= 4.0);
    }

    [Fact]
    public void Solve_NoGainNeeded_ResultEqualsCurrentGrades()
    {
        double[] current = { 7, 8 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 7.0 * 1.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.True(result[0] <= current[0] + 1e-6);
        Assert.True(result[1] <= current[1] + 1e-6);
    }

    [Fact]
    public void Solve_GradeNeverExceedsTen()
    {
        double[] current = { 0, 0 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 10.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.All(result, g => Assert.True(g <= 10.0 + 1e-6));
    }

    [Fact]
    public void Solve_BlockingThreshold3Point5_ExactValue()
    {
        double[] current = { 3, 5 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 6, 4 };
        double target = 6.0;
        bool[] isBlocking = { true, false };
        double[] minimumGrades = { 3.5, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.True(result[0] >= 3.5 - 1e-6);
    }

    [Fact]
    public void Solve_TwoBlockingComponents_BothRaised()
    {
        double[] current = { 2, 3 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 8.0;
        bool[] isBlocking = { true, true };
        double[] minimumGrades = { 4.0, 4.0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.True(result[0] >= 4.0 - 1e-6);
        Assert.True(result[1] >= 4.0 - 1e-6);
    }
    

    [Fact]
    public void Solve_TargetAlreadyExceeded_ReturnsCurrentGrades()
    {
        double[] current = { 8, 9 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 5.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.Equal(current[0], result[0], 5);
        Assert.Equal(current[1], result[1], 5);
    }

    [Fact]
    public void Solve_ZeroWeightComponent_Ignored()
    {
        double[] current = { 0, 0 };
        double[] weights = { 0.0, 1.0 };
        int[] complexity = { 5, 5 };
        double target = 7.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.Equal(0, result[0], 5);
        Assert.Equal(7.0, result[1], 5);
    }

    [Fact]
    public void Solve_BlockingExactlyAtMinimum_NotRaised()
    {
        double[] current = { 4.0, 0 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 3 };
        double target = 6.0;
        bool[] isBlocking = { true, false };
        double[] minimumGrades = { 4.0, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.Equal(4.0, result[0], 5);
    }

    [Fact]
    public void Solve_MultipleBlockingComponents_AllRaisedBeforeGreedy()
    {
        double[] current = { 2, 1, 0 };
        double[] weights = { 1.0 / 3, 1.0 / 3, 1.0 / 3 };
        int[] complexity = { 5, 5, 2 };
        double target = 7.0;
        bool[] isBlocking = { true, true, false };
        double[] minimumGrades = { 4.0, 3.5, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.True(result[0] >= 4.0 - 1e-6);
        Assert.True(result[1] >= 3.5 - 1e-6);
    }

    [Fact]
    public void Solve_BlockingRaisesRemaining_ReducesGap()
    {
        double[] current = { 0, 0 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 4.0;
        bool[] isBlocking = { true, false };
        double[] minimumGrades = { 4.0, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        double achievedSum = result[0] * 0.5 + result[1] * 0.5;
        Assert.True(achievedSum >= 4.0 - 1e-6);
    }

    [Fact]
    public void Solve_FourComponents_CorrectOrder()
    {
        double[] current = { 0, 0, 0, 0 };
        double[] weights = { 0.25, 0.25, 0.25, 0.25 };
        int[] complexity = { 8, 2, 6, 1 };
        double target = 3.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.True(result[3] >= result[1] - 1e-6);
        Assert.True(result[1] >= result[2] - 1e-6);
    }
    
    [Fact]
    public void Solve_SingleComponent_RaisedToTarget()
    {
        double[] current = { 4 };
        double[] weights = { 1.0 };
        int[] complexity = { 5 };
        double target = 7.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.Equal(7.0, result[0], precision: 5);
    }

    [Fact]
    public void Solve_AllComponentsAtMax_NoChange()
    {
        double[] current = { 10, 10 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 10.0;

        double[] result = Solve(current, weights, complexity, target);

        Assert.All(result, g => Assert.Equal(10.0, g, precision: 5));
    }

    [Fact]
    public void Solve_ThreeComponents_LowestCostFirst()
    {
        double[] current = { 0, 0, 0 };
        double[] weights = { 0.4, 0.4, 0.2 };
        int[] complexity = { 2, 5, 8 };
        double target = 5.0;

        double[] result = Solve(current, weights, complexity, target);
        
        Assert.True(result[0] >= result[1] - 1e-6);
    }

    [Fact]
    public void Solve_BlockingAlreadyAboveMinimum_NotForcedHigher()
    {
        double[] current = { 6, 0 };
        double[] weights = { 0.5, 0.5 };
        int[] complexity = { 5, 5 };
        double target = 7.0;
        bool[] isBlocking = { true, false };
        double[] minimumGrades = { 4.0, 0 };

        double[] result = Solve(current, weights, complexity, target, isBlocking, minimumGrades);

        Assert.True(result[0] >= 6.0 - 1e-6);
    }
    
    [Fact]
    public void ToDomain_MultipleComponents_PreservesOrder()
    {
        CreateSubjectRequest request = new CreateSubjectRequest
        {
            Name = "Тест",
            TargetGrade = 7,
            Components = new List<ComponentDto>
            {
                new ComponentDto { Name = "Первый", Weight = 40, Complexity = 3, CurrentGrade = 5 },
                new ComponentDto { Name = "Второй", Weight = 30, Complexity = 5, CurrentGrade = 4 },
                new ComponentDto { Name = "Третий", Weight = 30, Complexity = 7, CurrentGrade = 3 }
            }
        };

        Subject subject = SubjectMapper.ToDomain(request);

        Assert.Equal("Первый", subject.Components[0].Name);
        Assert.Equal("Второй", subject.Components[1].Name);
        Assert.Equal("Третий", subject.Components[2].Name);
    }

    [Fact]
    public void ToResponse_AutoGradeComponentAlreadyAchieved_IsAlreadyAchievedTrue()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Экзамен", 100, 5, 9, false, false, 0, true, 8.0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);
        OptimizationPlan planWithAuto = service.CalculateOptimizationPlanWithAuto(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan, null, planWithAuto);

        Assert.NotNull(response.PlanWithAuto);
        AutoGradeComponentInfoDto? info = response.PlanWithAuto.AutoGradeComponents
            .FirstOrDefault(c => c.ComponentName == "Экзамен");
        Assert.NotNull(info);
        Assert.True(info.IsAlreadyAchieved);
    }

    [Fact]
    public void ToResponse_AutoGradeComponentNotYetAchieved_IsAlreadyAchievedFalse()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 5,
            components: ("ДЗ", 100, 3, 4, false, false, 0, true, 8.0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);
        OptimizationPlan planWithAuto = service.CalculateOptimizationPlanWithAuto(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan, null, planWithAuto);

        Assert.NotNull(response.PlanWithAuto);
        AutoGradeComponentInfoDto? info = response.PlanWithAuto.AutoGradeComponents
            .FirstOrDefault(c => c.ComponentName == "ДЗ");
        Assert.NotNull(info);
        Assert.False(info.IsAlreadyAchieved);
    }

    [Fact]
    public void ToResponse_RecommendationMapped()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: ("Экзамен", 100, 5, 9, false, false, 0, false, 0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

        Assert.False(string.IsNullOrEmpty(response.Recommendation));
    }

    [Fact]
    public void ToResponse_CurrentGradeRounded()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: [
                ("А", 33, 5, 5, false, false, 0, false, 0),
                ("Б", 33, 5, 7, false, false, 0, false, 0),
                ("В", 34, 5, 6, false, false, 0, false, 0)
            ]);

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

        Assert.True(response.CurrentGrade >= 0 && response.CurrentGrade <= 10);
        string rounded = response.CurrentGrade.ToString("F10");
        int dotIdx = rounded.IndexOf('.');
        string decimals = dotIdx >= 0 ? rounded.Substring(dotIdx + 1).TrimEnd('0') : "";
        Assert.True(decimals.Length <= 2);
    }
}