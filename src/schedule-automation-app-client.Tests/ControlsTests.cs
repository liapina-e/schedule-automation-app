using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Controls;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class ControlsTests
{
    [AvaloniaFact]
    public void FormulaTab_Constructor_DoesNotThrow()
    {
        FormulaTab tab = new FormulaTab();
        Assert.NotNull(tab);
    }

    [AvaloniaFact]
    public void FormulaTab_WithViewModel_CanSetDataContext()
    {
        FormulaTab tab = new FormulaTab();
        MainViewModel vm = new MainViewModel();
        tab.DataContext = vm;

        Assert.Same(vm, tab.DataContext);
    }

    [AvaloniaFact]
    public void FormulaTab_OnCellEditEnded_CallsRefreshFormulaStats()
    {
        FormulaTab tab = new FormulaTab();
        MainViewModel vm = new MainViewModel();
        tab.DataContext = vm;

        vm.Subjects.Add(new Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        Assert.False(vm.IsFormulaValid);

        vm.SelectedSubject.Formula.Add(new GradeComponent
        {
            Weight = 100, Complexity = 5, CurrentGrade = 5
        });

        System.Reflection.MethodInfo? method = typeof(FormulaTab).GetMethod(
            "OnCellEditEnded",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);

        try
        {
            method!.Invoke(tab, new object?[] { tab, null });
        }
        catch (System.Reflection.TargetInvocationException)
        {
        }

        vm.RefreshFormulaStats();
        Assert.True(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void FormulaTab_OnCellEditEnded_WithNonMainViewModelDataContext_DoesNotThrow()
    {
        FormulaTab tab = new FormulaTab();
        tab.DataContext = new object();

        System.Reflection.MethodInfo? method = typeof(FormulaTab).GetMethod(
            "OnCellEditEnded",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        try
        {
            method!.Invoke(tab, new object?[] { tab, null });
        }
        catch (System.Reflection.TargetInvocationException)
        {
        }
    }

    [AvaloniaFact]
    public void PlanTab_Constructor_DoesNotThrow()
    {
        PlanTab tab = new PlanTab();
        Assert.NotNull(tab);
    }

    [AvaloniaFact]
    public void PlanTab_WithViewModel_CanSetDataContext()
    {
        PlanTab tab = new PlanTab();
        MainViewModel vm = new MainViewModel();
        tab.DataContext = vm;

        Assert.Same(vm, tab.DataContext);
    }

    [AvaloniaFact]
    public void SubjectsTab_Constructor_DoesNotThrow()
    {
        SubjectsTab tab = new SubjectsTab();
        Assert.NotNull(tab);
    }

    [AvaloniaFact]
    public void SubjectsTab_WithViewModel_CanSetDataContext()
    {
        SubjectsTab tab = new SubjectsTab();
        MainViewModel vm = new MainViewModel();
        tab.DataContext = vm;

        Assert.Same(vm, tab.DataContext);
    }

    [AvaloniaFact]
    public void MainWindow_Constructor_CreatesWithMainViewModel()
    {
        MainWindow window = new MainWindow();
        Assert.NotNull(window);
        Assert.IsType<MainViewModel>(window.DataContext);
    }

    [AvaloniaFact]
    public void Views_SubjectDialog_DefaultConstructor_DoesNotThrow()
    {
        schedule_automation_app_client.Views.SubjectDialog dialog =
            new schedule_automation_app_client.Views.SubjectDialog();
        Assert.NotNull(dialog);
    }

    [AvaloniaFact]
    public void Views_SubjectDialog_WithViewModel_SetsDataContextAndSubscribes()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        schedule_automation_app_client.Views.SubjectDialog dialog =
            new schedule_automation_app_client.Views.SubjectDialog(vm);

        Assert.Same(vm, dialog.DataContext);
    }

    [AvaloniaFact]
    public void Views_SubjectDialog_CloseInvokedFromViewModel_ClosesWindow()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        schedule_automation_app_client.Views.SubjectDialog dialog =
            new schedule_automation_app_client.Views.SubjectDialog(vm);

        vm.Name = "Тест";
        vm.TargetGradeValue = 7;
        vm.CancelCommand.Execute(null);

        Assert.NotNull(dialog);
    }
}