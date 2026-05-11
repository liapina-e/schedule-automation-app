using Avalonia.Headless.XUnit;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class FormulaValidationTests
{
    [AvaloniaFact]
    public void FormulaStatusColor_WhenInvalid_IsRed()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "А", Weight = 50, Complexity = 5, CurrentGrade = 5
        });
        vm.RefreshFormulaStats();

        Assert.Equal("#dc3545", vm.FormulaStatusColor);
    }

    [AvaloniaFact]
    public void TotalWeightMessage_WhenSumIs100_ContainsBothNumbers()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5
        });
        vm.RefreshFormulaStats();

        Assert.Contains("100", vm.TotalWeightMessage);
    }

    [AvaloniaFact]
    public void FormulaStatusColor_WhenValid_IsGreen()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5
        });
        vm.RefreshFormulaStats();

        Assert.Equal("#28a745", vm.FormulaStatusColor);
    }
}