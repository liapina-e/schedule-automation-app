using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Services;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class SubjectDialogViewModelTests
{
    [AvaloniaFact]
    public void SaveCommand_WhenNameEmpty_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = string.Empty;
        vm.TargetGradeValue = 6;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
    }

    [AvaloniaFact]
    public void SaveCommand_WhenNameOver100Chars_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = new string('а', 101);
        vm.TargetGradeValue = 6;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.NameError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenGradeBelow4_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 3;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.GradeError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenGradeAbove10_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 11;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.GradeError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenValidData_CanExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;

        Assert.True(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.Empty(vm.NameError);
        Assert.Empty(vm.GradeError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenAutoGradeEnabled_ResultHasAutoGrade()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Алгоритмы";
        vm.TargetGradeValue = 6;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = 8;

        ((RelayCommand)vm.SaveCommand).Execute(null);

        Assert.NotNull(vm.Result);
        Assert.True(vm.Result!.HasAutoGrade);
        Assert.Equal(8.0, vm.Result.AutoGradeMinScore);
    }
}