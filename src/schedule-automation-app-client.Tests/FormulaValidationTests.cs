using Avalonia.Headless.XUnit;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class FormulaValidationTests
{
    [AvaloniaFact]
    public void IsFormulaValid_WhenSumIs100_ReturnsTrue()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "Компонент А",
            Weight = 60,
            Complexity = 5,
            CurrentGrade = 5
        });

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "Компонент Б",
            Weight = 40,
            Complexity = 4,
            CurrentGrade = 4
        });

        vm.RefreshFormulaStats();

        Assert.True(vm.IsFormulaValid);
    }

    [AvaloniaFact]
    public void IsFormulaValid_WhenSumIsNot100_ReturnsFalse()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "Компонент А",
            Weight = 60,
            Complexity = 5,
            CurrentGrade = 5
        });

        vm.RefreshFormulaStats();

        Assert.False(vm.IsFormulaValid);
        Assert.Equal("#dc3545", vm.FormulaStatusColor);
    }

    [AvaloniaFact]
    public void CanCalculatePlan_WhenFormulaInvalid_ReturnsFalse()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "А",
            Weight = 70,
            Complexity = 5,
            CurrentGrade = 5
        });

        vm.RefreshFormulaStats();

        Assert.False(vm.CanCalculatePlan);
    }

    [AvaloniaFact]
    public void CanCalculatePlan_WhenFormulaValid_ReturnsTrue()
    {
        MainViewModel vm = new MainViewModel();
        vm.Subjects.Add(new Models.Subject { Name = "Тест", TargetGrade = 6 });
        vm.SelectedSubject = vm.Subjects[0];

        vm.SelectedSubject.Formula.Add(new Models.GradeComponent
        {
            Name = "А",
            Weight = 100,
            Complexity = 5,
            CurrentGrade = 5
        });

        vm.RefreshFormulaStats();

        Assert.True(vm.IsFormulaValid);
        Assert.Equal("#28a745", vm.FormulaStatusColor);
    }
}