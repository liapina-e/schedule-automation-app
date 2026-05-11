using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.ViewModels;

namespace schedule_automation_app_client.Tests;

public class MainViewModelTests
{
    [AvaloniaFact]
    public void CanSelectBlockingMinimum_FalseWhenNoComponentSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        Assert.False(vm.CanSelectBlockingMinimum);
    }

    [AvaloniaFact]
    public void CanSelectBlockingMinimum_FalseWhenComponentNotBlocking()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        GradeComponent component = new GradeComponent { IsBlocking = false };
        vm.SelectedSubject.Formula.Add(component);
        vm.SelectedComponent = component;

        Assert.False(vm.CanSelectBlockingMinimum);
    }

    [AvaloniaFact]
    public void CanSelectBlockingMinimum_TrueWhenComponentIsBlocking()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        GradeComponent component = new GradeComponent { IsBlocking = true };
        vm.SelectedSubject.Formula.Add(component);
        vm.SelectedComponent = component;

        Assert.True(vm.CanSelectBlockingMinimum);
    }

    [AvaloniaFact]
    public void CanAddComponent_TrueWhenSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        Assert.True(vm.CanAddComponent);
    }

    [AvaloniaFact]
    public void CanDeleteComponent_FalseWhenNoComponentSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        Assert.False(vm.CanDeleteComponent);
    }

    [AvaloniaFact]
    public void CanDeleteComponent_TrueWhenSubjectAndComponentSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        GradeComponent component = new GradeComponent();
        vm.SelectedSubject.Formula.Add(component);
        vm.SelectedComponent = component;

        Assert.True(vm.CanDeleteComponent);
    }

    [AvaloniaFact]
    public void AddComponentCommand_AddsComponentToFormula()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.AddComponentCommand.Execute(null);

        Assert.Single(vm.SelectedSubject.Formula);
    }

    [AvaloniaFact]
    public void DeleteComponentCommand_RemovesComponentFromFormula()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.AddComponentCommand.Execute(null);
        vm.SelectedComponent = vm.SelectedSubject.Formula[0];
        vm.DeleteComponentCommand.Execute(null);

        Assert.Empty(vm.SelectedSubject.Formula);
    }

    [AvaloniaFact]
    public void SelectedSubject_WhenChanged_ResetsCurrentPlan()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест1", TargetGrade = 6 });
        vm.Subjects.Add(new Subject { Name = "Тест2", TargetGrade = 7 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject = vm.Subjects[1];

        Assert.Null(vm.CurrentPlan);
    }

    [AvaloniaFact]
    public void CurrentFormula_WhenNoSubjectSelected_ReturnsNull()
    {
        MainViewModel vm = new MainViewModel();

        Assert.Null(vm.CurrentFormula);
    }

    [AvaloniaFact]
    public void CurrentFormula_WhenSubjectSelected_ReturnsFormula()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent());
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        Assert.NotNull(vm.CurrentFormula);
        Assert.Single(vm.CurrentFormula);
    }

    [AvaloniaFact]
    public void CanEditSubject_FalseWhenNoSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.CanEditSubject);
    }

    [AvaloniaFact]
    public void CanEditSubject_TrueWhenSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];
        Assert.True(vm.CanEditSubject);
    }

    [AvaloniaFact]
    public void CanDeleteSubject_FalseWhenNoSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.CanDeleteSubject);
    }

    [AvaloniaFact]
    public void CanDeleteSubject_TrueWhenSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];
        Assert.True(vm.CanDeleteSubject);
    }

    [AvaloniaFact]
    public void CanAddComponent_FalseWhenNoSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.CanAddComponent);
    }

    [AvaloniaFact]
    public void CanCalculatePlan_FalseWhenNoSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.CanCalculatePlan);
    }

    [AvaloniaFact]
    public void CanCalculatePlan_FalseWhenFormulaInvalid()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new GradeComponent { Name = "А", Weight = 60, Complexity = 5, CurrentGrade = 5 });
        vm.RefreshFormulaStats();

        Assert.False(vm.CanCalculatePlan);
    }

    [AvaloniaFact]
    public void TotalWeightMessage_WhenNoSubjectSelected_ReturnsSelectMessage()
    {
        MainViewModel vm = new MainViewModel();
        Assert.Equal("Выберите предмет", vm.TotalWeightMessage);
    }

    [AvaloniaFact]
    public void TotalWeightMessage_WhenSubjectWithComponents_ShowsTotal()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];
        vm.SelectedSubject.Formula.Add(new GradeComponent { Name = "А", Weight = 60, Complexity = 5, CurrentGrade = 5 });
        vm.RefreshFormulaStats();

        Assert.Contains("60", vm.TotalWeightMessage);
    }

    [AvaloniaFact]
    public void HasPlansRange_FalseWhenNoPlan()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.HasPlansRange);
    }

    [AvaloniaFact]
    public void HasAutoGradePlan_FalseWhenNoPlan()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.HasAutoGradePlan);
    }

    [AvaloniaFact]
    public void StatusMessage_WhenSubjectSelected_ContainsSubjectName()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Математика", TargetGrade = 7 });
        vm.SelectedSubject = vm.Subjects[0];

        Assert.Contains("Математика", vm.StatusMessage);
    }

    [AvaloniaFact]
    public void BlockingMinimumOptions_ReturnsTwoOptions()
    {
        MainViewModel vm = new MainViewModel();
        Assert.Equal(new[] { 3.5, 4.0 }, vm.BlockingMinimumOptions);
    }

    [AvaloniaFact]
    public void SelectedComponent_WhenSet_UpdatesCanDeleteComponent()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        GradeComponent component = new GradeComponent();
        vm.SelectedSubject.Formula.Add(component);

        vm.SelectedComponent = component;
        Assert.True(vm.CanDeleteComponent);

        vm.SelectedComponent = null;
        Assert.False(vm.CanDeleteComponent);
    }

    [AvaloniaFact]
    public void SelectedComponent_IsBlocking_ChangedAfterSet_UpdatesCanSelectBlockingMinimum()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        GradeComponent component = new GradeComponent { IsBlocking = false };
        vm.SelectedSubject.Formula.Add(component);
        vm.SelectedComponent = component;

        Assert.False(vm.CanSelectBlockingMinimum);

        component.IsBlocking = true;

        Assert.True(vm.CanSelectBlockingMinimum);
    }

    [AvaloniaFact]
    public void AddComponentCommand_AddsMultipleComponents()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        Assert.Equal(3, vm.SelectedSubject.Formula.Count);
    }

    [AvaloniaFact]
    public void IsFormulaValid_WhenNoComponents_ReturnsFalse()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.RefreshFormulaStats();

        Assert.False(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void IsFormulaValid_WhenThreeComponentsSum100_ReturnsTrue()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new GradeComponent { Name = "А", Weight = 50, Complexity = 5, CurrentGrade = 5 });
        vm.SelectedSubject.Formula.Add(new GradeComponent { Name = "Б", Weight = 30, Complexity = 3, CurrentGrade = 4 });
        vm.SelectedSubject.Formula.Add(new GradeComponent { Name = "В", Weight = 20, Complexity = 7, CurrentGrade = 6 });
        vm.RefreshFormulaStats();

        Assert.True(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void WhatIf_LoadedWhenSubjectSelected()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 7 };
        subject.Formula.Add(new GradeComponent { Name = "Экзамен", Weight = 100, Complexity = 5, CurrentGrade = 6 });
        vm.Subjects.Add(subject);

        vm.SelectedSubject = subject;

        Assert.Single(vm.WhatIf.Components);
        Assert.Equal("Экзамен", vm.WhatIf.Components[0].Name);
    }


    [AvaloniaFact]
    public void OnFormulaCollectionChanged_AfterAddComponent_RefreshesStats()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        Assert.False(vm.IsFormulaValid);

        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });

        Assert.True(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void OnFormulaCollectionChanged_AfterRemoveComponent_RefreshesStats()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        Assert.True(vm.IsFormulaValid);

        subject.Formula.RemoveAt(0);

        Assert.False(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void OnFormulaCollectionChanged_AfterAddComponent_UpdatesWhatIf()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        Assert.Empty(vm.WhatIf.Components);

        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });

        Assert.Single(vm.WhatIf.Components);
    }

    [AvaloniaFact]
    public void SelectedSubject_ChangeFromOneToAnother_UnsubscribesPreviousFormula()
    {
        MainViewModel vm = new MainViewModel();
        Subject first = new Subject { Name = "Первый", TargetGrade = 6 };
        Subject second = new Subject { Name = "Второй", TargetGrade = 7 };
        vm.Subjects.Add(first);
        vm.Subjects.Add(second);

        vm.SelectedSubject = first;
        vm.SelectedSubject = second;

        Assert.Equal(second, vm.SelectedSubject);
        Assert.Equal(second.Formula, vm.CurrentFormula);
    }

    [AvaloniaFact]
    public void SelectedSubject_SetToNull_CurrentFormulaIsNull()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent());
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        vm.SelectedSubject = null;

        Assert.Null(vm.CurrentFormula);
    }

    [AvaloniaFact]
    public void SelectedComponent_ChangeFromOneToAnother_UnsubscribesPrevious()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        GradeComponent first = new GradeComponent { Name = "А", IsBlocking = false };
        GradeComponent second = new GradeComponent { Name = "Б", IsBlocking = true };
        subject.Formula.Add(first);
        subject.Formula.Add(second);
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        vm.SelectedComponent = first;
        vm.SelectedComponent = second;

        Assert.True(vm.CanSelectBlockingMinimum);

        first.IsBlocking = false;
        Assert.True(vm.CanSelectBlockingMinimum);
    }

    [AvaloniaFact]
    public void UpdateStatusMessage_WithAutoGradeAchieved_ContainsCheckMark()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject
        {
            Name = "Математика",
            TargetGrade = 7,
            HasAutoGrade = true,
            AutoGradeMinScore = 6
        };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 8 });
        vm.Subjects.Add(subject);

        vm.SelectedSubject = subject;

        Assert.Contains("Автомат", vm.StatusMessage);
        Assert.Contains("✓", vm.StatusMessage);
    }

    [AvaloniaFact]
    public void UpdateStatusMessage_WithAutoGradeNotAchieved_ShowsRequiredScore()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject
        {
            Name = "Математика",
            TargetGrade = 7,
            HasAutoGrade = true,
            AutoGradeMinScore = 9
        };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);

        vm.SelectedSubject = subject;

        Assert.Contains("Автомат", vm.StatusMessage);
        Assert.Contains("нужно", vm.StatusMessage);
    }

    [AvaloniaFact]
    public void UpdateStatusMessage_WithoutAutoGrade_NoAutoInfo()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject
        {
            Name = "Математика",
            TargetGrade = 7,
            HasAutoGrade = false
        };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);

        vm.SelectedSubject = subject;

        Assert.DoesNotContain("Автомат", vm.StatusMessage);
    }

    [AvaloniaFact]
    public void UpdateStatusMessage_WithComponents_ShowsComponentCount()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 7 };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 50 });
        subject.Formula.Add(new GradeComponent { Name = "Б", Weight = 50 });
        vm.Subjects.Add(subject);

        vm.SelectedSubject = subject;

        Assert.Contains("2 компонентов", vm.StatusMessage);
    }

    [AvaloniaFact]
    public void IsLoading_DefaultValue_IsFalse()
    {
        MainViewModel vm = new MainViewModel();
        Assert.False(vm.IsLoading);
    }

    [AvaloniaFact]
    public void IsLoading_SetTrue_CanCalculatePlanFalse()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        Assert.True(vm.CanCalculatePlan);

        vm.IsLoading = true;

        Assert.False(vm.CanCalculatePlan);
    }

    [AvaloniaFact]
    public void ServerStatus_CanBeSetAndRetrieved()
    {
        MainViewModel vm = new MainViewModel();
        vm.ServerStatus = "Сервер доступен";
        Assert.Equal("Сервер доступен", vm.ServerStatus);
    }

    [AvaloniaFact]
    public void CurrentPlan_WhenSet_HasPlansRangeReflectsState()
    {
        MainViewModel vm = new MainViewModel();
        var plan = new schedule_automation_app_client.Services.Dtos.PlanResponseDto(
            Guid.NewGuid(), "Тест", 5, 7, true, "Рек",
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>(),
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.GradePlanDto>
            {
                new schedule_automation_app_client.Services.Dtos.GradePlanDto(7, true, "", new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>()),
                new schedule_automation_app_client.Services.Dtos.GradePlanDto(8, true, "", new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>())
            },
            false, null, null);

        vm.CurrentPlan = plan;

        Assert.True(vm.HasPlansRange);
    }

    [AvaloniaFact]
    public void CurrentPlan_WhenSingleItemRange_HasPlansRangeFalse()
    {
        MainViewModel vm = new MainViewModel();
        var plan = new schedule_automation_app_client.Services.Dtos.PlanResponseDto(
            Guid.NewGuid(), "Тест", 5, 7, true, "Рек",
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>(),
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.GradePlanDto>
            {
                new schedule_automation_app_client.Services.Dtos.GradePlanDto(7, true, "", new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>())
            },
            false, null, null);

        vm.CurrentPlan = plan;

        Assert.False(vm.HasPlansRange);
    }

    [AvaloniaFact]
    public void CurrentPlan_WhenAutoOption_HasAutoGradePlanTrue()
    {
        MainViewModel vm = new MainViewModel();
        var autoPlan = new schedule_automation_app_client.Services.Dtos.AutoGradePlanDto(
            true, "Авто",
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>(),
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.AutoGradeComponentInfoDto>());

        var plan = new schedule_automation_app_client.Services.Dtos.PlanResponseDto(
            Guid.NewGuid(), "Тест", 5, 7, true, "Рек",
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>(),
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.GradePlanDto>(),
            true, autoPlan, null);

        vm.CurrentPlan = plan;

        Assert.True(vm.HasAutoGradePlan);
    }

    [AvaloniaFact]
    public void CurrentPlan_WhenAutoOptionButNoPlanWithAuto_HasAutoGradePlanFalse()
    {
        MainViewModel vm = new MainViewModel();
        var plan = new schedule_automation_app_client.Services.Dtos.PlanResponseDto(
            Guid.NewGuid(), "Тест", 5, 7, true, "Рек",
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.OptimizationItemDto>(),
            new System.Collections.Generic.List<schedule_automation_app_client.Services.Dtos.GradePlanDto>(),
            true, null, null);

        vm.CurrentPlan = plan;

        Assert.False(vm.HasAutoGradePlan);
    }

    [AvaloniaFact]
    public void DeleteComponentCommand_WhenNoComponentSelected_DoesNothing()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent());
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.SelectedComponent = null;

        Assert.False(vm.CanDeleteComponent);
    }

    [AvaloniaFact]
    public void FormulaStatusColor_WhenValid_ReturnsGreen()
    {
        MainViewModel vm = new MainViewModel();
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent { Weight = 100, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        Assert.Equal("#28a745", vm.FormulaStatusColor);
    }
}