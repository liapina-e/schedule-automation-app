using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
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

	[AvaloniaFact]
    public void SaveCommand_WhenAutoGradeEnabledAndScoreBelow4_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = 3;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.AutoGradeError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenAutoGradeEnabledAndScoreAbove10_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = 11;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.AutoGradeError);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenAutoGradeDisabled_AutoGradeErrorIsEmpty()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = false;
        vm.AutoGradeMinScoreValue = 3;

        Assert.True(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.Empty(vm.AutoGradeError);
    }

    [AvaloniaFact]
    public void LoadSubject_SetsEditModeAndFields()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        Subject subject = new Subject
        {
            Name = "Физика",
            TargetGrade = 8,
            HasAutoGrade = true,
            AutoGradeMinScore = 9
        };

        vm.LoadSubject(subject);

        Assert.True(vm.IsEditMode);
        Assert.Equal("Редактировать предмет", vm.Title);
        Assert.Equal("Физика", vm.Name);
        Assert.Equal(8, vm.TargetGradeValue);
        Assert.True(vm.HasAutoGrade);
        Assert.Equal(9, vm.AutoGradeMinScoreValue);
    }

    [AvaloniaFact]
    public void CancelCommand_ResultRemainsNull()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;

        vm.CancelCommand.Execute(null);

        Assert.Null(vm.Result);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenAutoGradeDisabled_ResultHasZeroAutoGradeMinScore()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Физика";
        vm.TargetGradeValue = 6;
        vm.HasAutoGrade = false;
        vm.AutoGradeMinScoreValue = 9;

        ((RelayCommand)vm.SaveCommand).Execute(null);

        Assert.NotNull(vm.Result);
        Assert.False(vm.Result!.HasAutoGrade);
        Assert.Equal(0.0, vm.Result.AutoGradeMinScore);
    } 

    [AvaloniaFact]
    public void Title_WhenNotEditMode_ShowsAddTitle()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        Assert.Equal("Добавить предмет", vm.Title);
    }

    [AvaloniaFact]
    public void Title_WhenEditMode_ShowsEditTitle()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.IsEditMode = true;
        Assert.Equal("Редактировать предмет", vm.Title);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenValidData_ResultHasCorrectName()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "  Физика  ";
        vm.TargetGradeValue = 8;

        ((RelayCommand)vm.SaveCommand).Execute(null);

        Assert.NotNull(vm.Result);
        Assert.Equal("Физика", vm.Result!.Name);
    }

    [AvaloniaFact]
    public void SaveCommand_WhenValidData_ResultHasCorrectTargetGrade()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Химия";
        vm.TargetGradeValue = 9;

        ((RelayCommand)vm.SaveCommand).Execute(null);

        Assert.NotNull(vm.Result);
        Assert.Equal(9, vm.Result!.TargetGrade);
    }

    [AvaloniaFact]
    public void TargetGradeValue_WhenNull_CannotExecuteSave()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = null;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
    }

    [AvaloniaFact]
    public void AutoGradeMinScoreValue_WhenNull_AndAutoGradeEnabled_CannotExecute()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Математика";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = null;

        Assert.False(((RelayCommand)vm.SaveCommand).CanExecute(null));
        Assert.NotEmpty(vm.AutoGradeError);
    }

    [AvaloniaFact]
    public void NameError_WhenNameIsWhitespace_ShowsError()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "   ";

        Assert.NotEmpty(vm.NameError);
    }

    [AvaloniaFact]
    public void GradeError_WhenGradeIs4_IsEmpty()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Тест";
        vm.TargetGradeValue = 4;

        Assert.Empty(vm.GradeError);
    }

    [AvaloniaFact]
    public void GradeError_WhenGradeIs10_IsEmpty()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Тест";
        vm.TargetGradeValue = 10;

        Assert.Empty(vm.GradeError);
    }

    [AvaloniaFact]
    public void AutoGradeError_WhenMinScoreIs4_IsEmpty()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Тест";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = 4;

        Assert.Empty(vm.AutoGradeError);
        Assert.True(((RelayCommand)vm.SaveCommand).CanExecute(null));
    }

    [AvaloniaFact]
    public void AutoGradeError_WhenMinScoreIs10_IsEmpty()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Тест";
        vm.TargetGradeValue = 7;
        vm.HasAutoGrade = true;
        vm.AutoGradeMinScoreValue = 10;

        Assert.Empty(vm.AutoGradeError);
    }

    [AvaloniaFact]
    public void LoadSubject_WithoutAutoGrade_SetsHasAutoGradeFalse()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        Subject subject = new Subject
        {
            Name = "Биология",
            TargetGrade = 6,
            HasAutoGrade = false,
            AutoGradeMinScore = 0
        };

        vm.LoadSubject(subject);

        Assert.False(vm.HasAutoGrade);
    }

    [AvaloniaFact]
    public void CloseRequested_FiredOnSave()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.Name = "Физика";
        vm.TargetGradeValue = 7;

        bool? result = null;
        vm.CloseRequested += r => result = r;

        ((RelayCommand)vm.SaveCommand).Execute(null);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void CloseRequested_FiredOnCancel()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();

        bool? result = null;
        vm.CloseRequested += r => result = r;

        vm.CancelCommand.Execute(null);

        Assert.False(result);
    }
}