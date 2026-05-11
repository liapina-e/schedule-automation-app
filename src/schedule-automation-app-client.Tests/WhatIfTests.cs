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

        Assert.Equal(7.0, vm.HypotheticalGrade, 1);
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

        Assert.Equal("#28a745", vm.HypotheticalGradeColor);
    }

    [AvaloniaFact]
    public void Recalculate_TwoAutoGradeComponents_AchievedOnlyWhenBothAboveThreshold()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ",      50, 5, false, false, 0, true, 7.0),
            ("Семинар", 50, 5, false, false, 0, true, 6.0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 6.9;
        vm.Components[1].HypotheticalGrade = 6.0;
        Assert.False(vm.AutoGradeAchieved);

        vm.Components[0].HypotheticalGrade = 7.0;
        Assert.True(vm.AutoGradeAchieved);
    }

    [AvaloniaFact]
    public void Recalculate_BlockingExactly4Point0_NotViolatedAtBoundary()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба",   50, 5, false, true,  4.0, false, 0),
            ("Теория", 50, 5, false, false, 0,   false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 4.0;
        Assert.False(vm.IsBlockingConditionFailed);

        vm.Components[0].HypotheticalGrade = 3.99;
        Assert.True(vm.IsBlockingConditionFailed);
    }

    [AvaloniaFact]
    public void Recalculate_GradeColorRed_WhenBlockingViolated_EvenIfGradeAbove6()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба",   20, 8, false, true,  4.0, false, 0),
            ("Теория", 80, 8, false, false, 0,   false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 3.0;

        Assert.True(vm.HypotheticalGrade >= 6);
        Assert.True(vm.IsBlockingConditionFailed);
        Assert.Equal("#dc3545", vm.HypotheticalGradeColor);
    }

    [AvaloniaFact]
    public void Recalculate_MultipleBlockingComponents_DifferentThresholds_FailsOnViolation()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба",     34, 5, false, true,  4.0, false, 0),
            ("Практика", 33, 5, false, true,  3.5, false, 0),
            ("Теория",   33, 5, false, false, 0,   false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 4.0;
        vm.Components[1].HypotheticalGrade = 3.5;
        Assert.False(vm.IsBlockingConditionFailed);

        vm.Components[1].HypotheticalGrade = 3.4;
        Assert.True(vm.IsBlockingConditionFailed);
        Assert.Contains("Практика", vm.BlockingWarning);

        vm.Components[1].HypotheticalGrade = 3.5;
        vm.Components[0].HypotheticalGrade = 3.9;
        Assert.True(vm.IsBlockingConditionFailed);
        Assert.Contains("Лаба", vm.BlockingWarning);
    }

    [AvaloniaFact]
    public void Recalculate_GradedComponentAboveThreshold_DoesNotTriggerBlockingWarning()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Лаба",   50, 5.0, true,  true,  4.0, false, 0),
            ("Теория", 50, 5.0, false, false, 0,   false, 0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 2.0;

        Assert.Equal(5.0, vm.Components[0].HypotheticalGrade);
        Assert.False(vm.IsBlockingConditionFailed);
        Assert.Empty(vm.BlockingWarning);
    }

    [AvaloniaFact]
    public void AutoGradeStatus_EmptyWhenNoAutoGradeComponents()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 7, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        Assert.Equal(string.Empty, vm.AutoGradeStatus);
    }

    [AvaloniaFact]
    public void AutoGradeStatus_ShowsAchievedText_WhenAutoGradeAchieved()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 8, false, false, 0, true, 7.0));

        vm.LoadFromSubject(subject);

        Assert.True(vm.AutoGradeAchieved);
        Assert.Contains("выполнено", vm.AutoGradeStatus);
    }

    [AvaloniaFact]
    public void AutoGradeStatus_ShowsNotAchievedText_WhenBelowThreshold()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 5, false, false, 0, true, 7.0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 6.9;

        Assert.False(vm.AutoGradeAchieved);
        Assert.Contains("не выполнено", vm.AutoGradeStatus);
    }

    [AvaloniaFact]
    public void AutoGradeStatusColor_GreenWhenAchieved()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 8, false, false, 0, true, 7.0));

        vm.LoadFromSubject(subject);

        Assert.Equal("#28a745", vm.AutoGradeStatusColor);
    }

    [AvaloniaFact]
    public void AutoGradeStatusColor_YellowWhenNotAchieved()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 5, false, false, 0, true, 7.0));

        vm.LoadFromSubject(subject);

        vm.Components[0].HypotheticalGrade = 6.0;

        Assert.Equal("#856404", vm.AutoGradeStatusColor);
    }

    [AvaloniaFact]
    public void LoadFromSubject_UpdatesHasAutoGradeComponents()
    {
        WhatIfViewModel vm = new WhatIfViewModel();

        Subject noAuto = BuildSubject(("Экзамен", 100, 7, false, false, 0, false, 0));
        vm.LoadFromSubject(noAuto);
        Assert.False(vm.HasAutoGradeComponents);

        Subject withAuto = BuildSubject(("ДЗ", 100, 7, false, false, 0, true, 7.0));
        vm.LoadFromSubject(withAuto);
        Assert.True(vm.HasAutoGradeComponents);
    }
    
    [AvaloniaFact]
    public void HypotheticalGradeText_ContainsFormattedGrade()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 7.5, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);

        Assert.Contains("7,5", vm.HypotheticalGradeText);
    }

    [AvaloniaFact]
    public void LoadFromSubject_EmptyFormula_HypotheticalGradeIsZero()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = new Subject { Name = "Пустой", TargetGrade = 7 };

        vm.LoadFromSubject(subject);

        Assert.Equal(0, vm.HypotheticalGrade);
    }

    [AvaloniaFact]
    public void LoadFromSubject_ReplacesExistingComponents()
    {
        WhatIfViewModel vm = new WhatIfViewModel();

        Subject first = BuildSubject(
            ("А", 50, 5, false, false, 0, false, 0),
            ("Б", 50, 5, false, false, 0, false, 0));
        vm.LoadFromSubject(first);
        Assert.Equal(2, vm.Components.Count);

        Subject second = BuildSubject(
            ("В", 100, 7, false, false, 0, false, 0));
        vm.LoadFromSubject(second);

        Assert.Single(vm.Components);
        Assert.Equal("В", vm.Components[0].Name);
    }

    [AvaloniaFact]
    public void Recalculate_HypotheticalGrade_ClampedAt10()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);
        vm.Components[0].HypotheticalGrade = 15;

        Assert.Equal(10, vm.Components[0].HypotheticalGrade);
    }

    [AvaloniaFact]
    public void Recalculate_HypotheticalGrade_ClampedAt0()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));

        vm.LoadFromSubject(subject);
        vm.Components[0].HypotheticalGrade = -5;

        Assert.Equal(0, vm.Components[0].HypotheticalGrade);
    }

    [AvaloniaFact]
    public void OnSourceComponentPropertyChanged_IsGraded_SyncsAndRecalculates()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));
        vm.LoadFromSubject(subject);

        subject.Formula[0].IsGraded = true;

        Assert.True(vm.Components[0].IsGraded);
    }

    [AvaloniaFact]
    public void OnSourceComponentPropertyChanged_IsBlocking_SyncsAndRecalculates()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));
        vm.LoadFromSubject(subject);

        subject.Formula[0].IsBlocking = true;
        subject.Formula[0].BlockingMinimum = 4.0;

        Assert.True(vm.Components[0].IsBlocking);
    }

    [AvaloniaFact]
    public void OnSourceComponentPropertyChanged_AutoGradeMinScore_SyncsAndRecalculates()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("ДЗ", 100, 5, false, false, 0, true, 7.0));
        vm.LoadFromSubject(subject);

        subject.Formula[0].AutoGradeMinScore = 8.0;

        Assert.Equal(8.0, vm.Components[0].AutoGradeMinScore);
    }

    [AvaloniaFact]
    public void OnSourceComponentPropertyChanged_CurrentGrade_SyncsGrade()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));
        vm.LoadFromSubject(subject);

        subject.Formula[0].CurrentGrade = 8;

        Assert.Equal(8, vm.Components[0].CurrentGrade);
    }

    [AvaloniaFact]
    public void AutoGradeStatusColor_IsGreenWhenNoAutoComponents()
    {
        WhatIfViewModel vm = new WhatIfViewModel();
        Subject subject = BuildSubject(
            ("Экзамен", 100, 5, false, false, 0, false, 0));
        vm.LoadFromSubject(subject);

        Assert.Equal("#28a745", vm.AutoGradeStatusColor);
    }

    [AvaloniaFact]
    public void WhatIfComponentViewModel_Constructor_SetsAllFields()
    {
        GradeComponent component = new GradeComponent
        {
            Name = "Экзамен",
            Weight = 60,
            Complexity = 7,
            CurrentGrade = 5,
            IsGraded = false,
            IsBlocking = true,
            BlockingMinimum = 4.0,
            IsAutoGrade = true,
            AutoGradeMinScore = 8.0
        };

        WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);

        Assert.Equal("Экзамен", vm.Name);
        Assert.Equal(60, vm.Weight);
        Assert.Equal(5, vm.CurrentGrade);
        Assert.False(vm.IsGraded);
        Assert.True(vm.IsBlocking);
        Assert.Equal(4.0, vm.BlockingMinimum);
        Assert.True(vm.IsAutoGrade);
        Assert.Equal(8.0, vm.AutoGradeMinScore);
        Assert.Equal(5, vm.HypotheticalGrade);
    }

    [AvaloniaFact]
    public void WhatIfComponentViewModel_GradeChanged_FiredWhenGradeChanges()
    {
        GradeComponent component = new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 };
        WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);

        bool fired = false;
        vm.GradeChanged += () => fired = true;

        vm.HypotheticalGrade = 7;

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void WhatIfComponentViewModel_GradeChanged_NotFiredWhenSameValue()
    {
        GradeComponent component = new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 };
        WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);

        int firedCount = 0;
        vm.GradeChanged += () => firedCount++;

        vm.HypotheticalGrade = 5;

        Assert.Equal(0, firedCount);
    }

    [AvaloniaFact]
    public void WhatIfComponentViewModel_SyncFromSource_UpdatesAllFlags()
    {
        GradeComponent component = new GradeComponent
        {
            Name = "Экзамен", Weight = 100, Complexity = 5, CurrentGrade = 4,
            IsGraded = false, IsBlocking = false, IsAutoGrade = false,
            AutoGradeMinScore = 0, BlockingMinimum = 4.0
        };
        WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);

        component.IsGraded = true;
        component.IsBlocking = true;
        component.IsAutoGrade = true;
        component.AutoGradeMinScore = 7.0;
        component.BlockingMinimum = 3.5;
        component.CurrentGrade = 6;

        vm.SyncFromSource();

        Assert.True(vm.IsGraded);
        Assert.True(vm.IsBlocking);
        Assert.True(vm.IsAutoGrade);
        Assert.Equal(7.0, vm.AutoGradeMinScore);
        Assert.Equal(3.5, vm.BlockingMinimum);
        Assert.Equal(6, vm.CurrentGrade);
    }

    [AvaloniaFact]
    public void WhatIfComponentViewModel_SyncFromSource_WhenGraded_SetsHypotheticalToCurrentGrade()
    {
        GradeComponent component = new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 4 };
        WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);
        vm.HypotheticalGrade = 8;

        component.IsGraded = true;
        component.CurrentGrade = 9;
        vm.SyncFromSource();

        Assert.Equal(9, vm.HypotheticalGrade);
    }
}