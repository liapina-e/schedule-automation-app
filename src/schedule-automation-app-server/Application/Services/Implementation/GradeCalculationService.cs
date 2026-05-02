using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services.Interfaces;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Implementation;

public class GradeCalculationService : IGradeCalculationService
{
    private readonly EffortMinimizationSolver _solver;
    private readonly ILogger<GradeCalculationService> _logger;

    public GradeCalculationService(ILogger<GradeCalculationService> logger)
    {
        _solver = new EffortMinimizationSolver();
        _logger = logger;
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

        double totalWeight = subject.Components.Sum(c => c.WeightAsDecimal());

        if (totalWeight < 1e-9)
        {
            return 0;
        }

        return subject.Components.Sum(c => c.CurrentGrade * c.WeightAsDecimal()) / totalWeight;
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

        _logger.LogInformation(
            "Расчёт плана для предмета '{Name}' (Id={Id}): текущая={Current:F2}, цель={Target}, максимум={Max:F2}",
            subject.Name, subject.Id, currentGrade, subject.TargetGrade, maxAchievable);

        if (maxAchievable < subject.TargetGrade - 1e-6)
        {
            _logger.LogInformation(
                "Предмет '{Name}': цель недостижима, максимум {Max:F2}",
                subject.Name, maxAchievable);

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
            _logger.LogInformation(
                "Предмет '{Name}': цель уже достигнута, текущая оценка {Current:F2}",
                subject.Name, currentGrade);

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

        _logger.LogInformation(
            "Предмет '{Name}': план рассчитан, {Count} компонентов к улучшению",
            subject.Name, items.Count);

        return new OptimizationPlan(
            subject: subject,
            currentGrade: currentGrade,
            necessaryPoints: gap,
            isAchievable: true,
            items: items,
            recommendation: recommendation
        );
    }

    public WhatIfResponse CalculateWhatIf(Subject subject, List<WhatIfComponentDto> hypotheticalGrades)
    {
        if (subject == null)
        {
            throw new ArgumentNullException(nameof(subject));
        }

        double currentGrade = CalculateCurrentGrade(subject);
        double totalWeight = subject.Components.Sum(c => c.WeightAsDecimal());

        if (totalWeight < 1e-9)
        {
            return new WhatIfResponse
            {
                CurrentGrade = currentGrade,
                HypotheticalGrade = currentGrade,
                TargetGrade = subject.TargetGrade,
                WouldAchieveTarget = currentGrade >= subject.TargetGrade,
                PointsRemaining = Math.Max(0, subject.TargetGrade - currentGrade)
            };
        }

        double hypotheticalWeightedSum = 0;
        List<WhatIfComponentResultDto> componentResults = new List<WhatIfComponentResultDto>();

        foreach (GradeComponent component in subject.Components)
        {
            double gradeToUse = component.CurrentGrade;

            if (!component.IsGraded)
            {
                WhatIfComponentDto? hypothesis = hypotheticalGrades
                    .FirstOrDefault(h => h.ComponentId == component.Id);

                if (hypothesis != null)
                {
                    gradeToUse = Math.Max(0, Math.Min(10, hypothesis.HypotheticalGrade));
                }
            }

            double contribution = gradeToUse * component.WeightAsDecimal();
            hypotheticalWeightedSum += contribution;

            componentResults.Add(new WhatIfComponentResultDto
            {
                ComponentId = component.Id,
                ComponentName = component.Name,
                CurrentGrade = component.CurrentGrade,
                HypotheticalGrade = gradeToUse,
                IsGraded = component.IsGraded,
                WeightedContribution = Math.Round(contribution, 3)
            });
        }

        double hypotheticalGrade = Math.Round(hypotheticalWeightedSum / totalWeight, 2);
        double pointsRemaining = Math.Max(0, subject.TargetGrade - hypotheticalGrade);

        _logger.LogInformation(
            "Сценарий что если для предмета '{Name}': гипотетическая оценка {Hyp:F2}, цель {Target}",
            subject.Name, hypotheticalGrade, subject.TargetGrade);

        return new WhatIfResponse
        {
            CurrentGrade = Math.Round(currentGrade, 2),
            HypotheticalGrade = hypotheticalGrade,
            TargetGrade = subject.TargetGrade,
            WouldAchieveTarget = hypotheticalGrade >= subject.TargetGrade - 1e-6,
            PointsRemaining = Math.Round(pointsRemaining, 2),
            Components = componentResults
        };
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
            .Where(c => c.CanBeImproved())
            .ToList();
    }

    private SolverInput PrepareInputData(Subject subject, List<GradeComponent> improvable)
    {
        double[] currentGrades = improvable.Select(c => c.CurrentGrade).ToArray();
        double[] weights = improvable.Select(c => c.WeightAsDecimal()).ToArray();
        int[] complexities = improvable.Select(c => c.Complexity).ToArray();

        double lockedSum = subject.Components
            .Except(improvable)
            .Sum(c => c.CurrentGrade * c.WeightAsDecimal());

        double totalWeight = subject.Components.Sum(c => c.WeightAsDecimal());
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

            if (needed <= component.CurrentGrade + 1e-6)
            {
                continue;
            }

            double requiredGrade = Math.Min(Math.Ceiling(needed * 100) / 100.0, 10.0);
            double costPerUnit = (double)component.Complexity / component.WeightAsDecimal();
            string reason = $"Стоимость усилий: {costPerUnit:F1} " +
                            $"(вес {component.Weight}%, сложность {component.Complexity})";

            items.Add(new OptimizationItem(component, requiredGrade, priority, reason));
            priority++;
        }

        return items;
    }

    private double CalculateMaxAchievableGrade(Subject subject)
    {
        double totalWeight = subject.Components.Sum(c => c.WeightAsDecimal());

        if (totalWeight < 1e-9)
        {
            return 0;
        }

        return subject.Components.Sum(c =>
        {
            double maxGrade = c.IsGraded ? c.CurrentGrade : 10.0;
            return maxGrade * c.WeightAsDecimal();
        }) / totalWeight;
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