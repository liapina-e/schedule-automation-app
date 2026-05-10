using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.ViewModels;

namespace schedule_automation_app_server.Tests;

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
}