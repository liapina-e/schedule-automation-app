using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class WhatIfTests
{
    private static Subject BuildSubject(params (string name, double weight, double current, bool isGraded, bool isBlocking, double blockingMin, bool isAutoGrade, double autoMin)[] components)
    {
        Subject subject = new Subject { Name = "Тест", TargetGrade = 7 };

        foreach (var c in components)
        {
            subject.Formula.Add(new GradeComponent
            {
                Name = c.name,
                Weight = c.weight,
                Complexity = 5,
                CurrentGrade = c.current,
                IsGraded = c.isGraded,
                IsBlocking = c.isBlocking,
                BlockingMinimum = c.blockingMin,
                IsAutoGrade = c.isAutoGrade,
                AutoGradeMinScore = c.autoMin
            });
        }

        return subject;
    }

    [AvaloniaFact]
    public void Recalculate_BasicWeightedAverage_IsCorrect()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 50, 8, false, false, 0, false, 0),
            ("ДЗ", 50, 6, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        Assert.Equal(7.0, vm.HypotheticalGrade, precision: 1);
    }

    [AvaloniaFact]
    public void Recalculate_GradedComponentIsLocked()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Зачёт", 40, 9, true, false, 0, false, 0),
            ("Проект", 60, 4, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        WhatIfComponentViewModel graded = vm.Components[0];
        graded.HypotheticalGrade = 3;

        Assert.Equal(9.0, graded.HypotheticalGrade);
    }

    [AvaloniaFact]
    public void Recalculate_BlockingViolation_SetsError()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба", 50, 5, false, true, 4.0, false, 0),
            ("Теория", 50, 5, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 3.0;

        Assert.True(vm.IsBlockingConditionFailed);
        Assert.Contains("Лаба", vm.BlockingWarning);
        Assert.Contains("4", vm.BlockingWarning);
    }

    [AvaloniaFact]
    public void Recalculate_BlockingViolation_Threshold3Point5()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба", 50, 5, false, true, 3.5, false, 0),
            ("Теория", 50, 5, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 3.4;
        Assert.True(vm.IsBlockingConditionFailed);

        vm.Components[0].HypotheticalGrade = 3.5;
        Assert.False(vm.IsBlockingConditionFailed);
    }

    [AvaloniaFact]
    public void Recalculate_AutoGradeAchieved_WhenAboveThreshold()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 4, false, false, 0, true, 7.0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 6.9;
        Assert.False(vm.AutoGradeAchieved);

        vm.Components[0].HypotheticalGrade = 7.0;
        Assert.True(vm.AutoGradeAchieved);
    }

    [AvaloniaFact]
    public void Recalculate_GradeColor_GreenWhenAbove6()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Компонент", 100, 7, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        Assert.Equal("#28a745", vm.HypotheticalGradeColor);
    }

    [AvaloniaFact]
    public void Recalculate_GradeColor_RedWhenBelow6()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Компонент", 100, 4, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        Assert.Equal("#dc3545", vm.HypotheticalGradeColor);
    }
}