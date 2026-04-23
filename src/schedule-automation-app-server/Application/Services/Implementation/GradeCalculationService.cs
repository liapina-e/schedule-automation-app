using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Implementation;

public class GradeCalculationService : IGradeCalculationService
{
    private readonly EffortMinimizationSolver _solver;

    public GradeCalculationService()
    {
        _solver = new EffortMinimizationSolver();
    }

    public double CalculateCurrentGrade(Subject subject)
    {
        if (subject == null)
        {
            throw new ArgumentNullException(nameof(subject));
        }

        if (subject.Components.Count == 0)
        {
            return 0;
        }

        double totalWeight = subject.Components.Sum(c => c.Weight.AsDecimal());

        if (totalWeight < 1e-9)
        {
            return 0;
        }

        double weightedSum = subject.Components
            .Sum(c => c.CurrentGrade.Value * c.Weight.AsDecimal());

        return weightedSum / totalWeight;
    }

    public OptimizationPlan CalculateOptimizationPlan(Subject subject)
    {
        if (subject == null)
        {
            throw new ArgumentNullException(nameof(subject));
        }

        double currentGrade = CalculateCurrentGrade(subject);
        double gap = Math.Max(0, subject.TargetGrade - currentGrade);
        double maxAchievable = CalculateMaxAchievableGrade(subject);

        if (maxAchievable < subject.TargetGrade - 1e-6)
        {
            return new OptimizationPlan(
                subject: subject,
                currentGrade: currentGrade,
                necessaryPoints: gap,
                isAchievable: false,
                items: new List<OptimizationItem>(),
                recommendation: $"Цель недостижима. Максимально возможная оценка: {maxAchievable:F2}."
            );
        }

        if (gap < 1e-9)
        {
            return new OptimizationPlan(
                subject: subject,
                currentGrade: currentGrade,
                necessaryPoints: 0,
                isAchievable: true,
                items: new List<OptimizationItem>(),
                recommendation: "Текущих оценок уже достаточно для достижения цели."
            );
        }

        List<OptimizationItem> items = BuildPlan(subject);
        string recommendation = BuildRecommendation(items);

        return new OptimizationPlan(
            subject: subject,
            currentGrade: currentGrade,
            necessaryPoints: gap,
            isAchievable: true,
            items: items,
            recommendation: recommendation
        );
    }

    private List<OptimizationItem> BuildPlan(Subject subject)
    {
        List<GradeComponent> improvable = GetImprovableComponents(subject);

        if (improvable.Count == 0)
        {
            return new List<OptimizationItem>();
        }

        SolverInput input = PrepareInputData(subject, improvable);
        double[] optimalGrades = _solver.Solve(
            input.CurrentGrades,
            input.Weights,
            input.Complexities,
            input.TargetWeightedSum
        );

        return MapResultToItems(improvable, optimalGrades);
    }

    private List<GradeComponent> GetImprovableComponents(Subject subject)
    {
        return subject.Components
            .Where(c => c.CurrentGrade.Value < 10.0 - 1e-6)
            .ToList();
    }

    private SolverInput PrepareInputData(Subject subject, List<GradeComponent> improvable)
    {
        double[] currentGrades = improvable.Select(c => c.CurrentGrade.Value).ToArray();
        double[] weights = improvable.Select(c => c.Weight.AsDecimal()).ToArray();
        int[] complexities = improvable.Select(c => c.Complexity.Value).ToArray();

        double lockedSum = subject.Components
            .Except(improvable)
            .Sum(c => c.CurrentGrade.Value * c.Weight.AsDecimal());

        double totalWeight = subject.Components.Sum(c => c.Weight.AsDecimal());
        double targetWeightedSum = subject.TargetGrade * totalWeight - lockedSum;

        return new SolverInput(currentGrades, weights, complexities, targetWeightedSum);
    }

    private List<OptimizationItem> MapResultToItems(List<GradeComponent> improvable, double[] optimalGrades)
    {
        List<OptimizationItem> items = new List<OptimizationItem>();
        int priority = 1;

        for (int i = 0; i < improvable.Count; i++)
        {
            GradeComponent component = improvable[i];
            double needed = optimalGrades[i];

            if (needed <= component.CurrentGrade.Value + 1e-6)
            {
                continue;
            }

            double requiredGrade = Math.Ceiling(needed * 100) / 100.0;
            requiredGrade = Math.Min(requiredGrade, 10.0);

            double costPerUnit = (double)component.Complexity.Value / component.Weight.AsDecimal();
            string reason = $"Стоимость усилий: {costPerUnit:F1} " +
                            $"(вес {component.Weight.Value}%, сложность {component.Complexity.Value})";

            items.Add(new OptimizationItem(component, requiredGrade, priority, reason));
            priority++;
        }

        return items;
    }

    private double CalculateMaxAchievableGrade(Subject subject)
    {
        double totalWeight = subject.Components.Sum(c => c.Weight.AsDecimal());

        if (totalWeight < 1e-9)
        {
            return 0;
        }

        return subject.Components.Sum(c => 10.0 * c.Weight.AsDecimal()) / totalWeight;
    }

    private string BuildRecommendation(List<OptimizationItem> items)
    {
        if (items.Count == 0)
        {
            return "Текущих оценок уже достаточно для достижения цели.";
        }

        IEnumerable<string> lines = items
            .OrderBy(i => i.Priority)
            .Select(i => $"{i.Priority}. {i.ComponentName}: " +
                         $"с {i.CurrentGrade.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} " +
                         $"до {i.RequiredGrade.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");

        return "Минимальный план:\n" + string.Join("\n", lines);
    }
}