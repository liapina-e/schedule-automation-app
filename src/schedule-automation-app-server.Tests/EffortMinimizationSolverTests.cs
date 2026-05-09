using schedule_automation_app_server.Application.Services.Implementation;

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
}