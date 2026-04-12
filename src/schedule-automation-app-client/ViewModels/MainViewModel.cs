using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;
using schedule_automation_app_client.Views;

namespace schedule_automation_app_client.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Subject _selectedSubject;
    private string _statusMessage;
    private ObservableCollection<Subject> _subjects;
    private GradeComponent _selectedComponent;

    public ObservableCollection<Subject> Subjects
    {
        get => _subjects;
        set => SetField(ref _subjects, value);
    }

    public Subject SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            if (SetField(ref _selectedSubject, value))
            {
                UpdateStatusMessage();
                RaiseCanExecuteForCommands();
            }
        }
    }

    public GradeComponent SelectedComponent
    {
        get => _selectedComponent;
        set
        {
            if (SetField(ref _selectedComponent, value))
            {
                (DeleteComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanDeleteComponent));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public string TotalWeightMessage
    {
        get
        {
            if (SelectedSubject == null || SelectedSubject.Formula == null)
            {
                return "Выберите предмет";
            }

            double totalWeight = SelectedSubject.Formula.Sum(c => c.Weight);
            return $"Сумма весов: {totalWeight}% из 100%";
        }
    }

    public bool IsFormulaValid
    {
        get
        {
            if (SelectedSubject?.Formula == null)
            {
                return false;
            }

            double sum = SelectedSubject.Formula.Sum(c => c.Weight);
            return Math.Abs(sum - 100) < 0.001;
        }
    }

    public string FormulaStatusColor => IsFormulaValid ? "#28a745" : "#dc3545";

    public bool CanEditSubject => SelectedSubject != null;
    public bool CanDeleteSubject => SelectedSubject != null;
    public bool CanAddComponent => SelectedSubject != null;
    public bool CanDeleteComponent => SelectedSubject != null && SelectedComponent != null;

    public ICommand AddSubjectCommand { get; set; }
    public ICommand EditSubjectCommand { get; set; }
    public ICommand DeleteSubjectCommand { get; set; }
    public ICommand AddComponentCommand { get; set; }
    public ICommand DeleteComponentCommand { get; set; }

    public MainViewModel()
    {
        InitializeTestData();
        InitializeCommands();
        StatusMessage = "Готово к работе. Выберите предмет или добавьте новый.";
    }

    private void InitializeTestData()
    {
        Subjects = new ObservableCollection<Subject>
        {
            new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Математический анализ",
                TargetGrade = 8,
                CreatedAt = DateTime.Now.AddDays(-5),
                Formula = new List<GradeComponent>
                {
                    new GradeComponent { Name = "Домашняя работа 1", Weight = 15, Complexity = 3 },
                    new GradeComponent { Name = "Домашняя работа 2", Weight = 15, Complexity = 3 },
                    new GradeComponent { Name = "Контрольная работа", Weight = 30, Complexity = 5 },
                    new GradeComponent { Name = "Экзамен", Weight = 40, Complexity = 8 }
                }
            },
            new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Физика",
                TargetGrade = 7,
                CreatedAt = DateTime.Now.AddDays(-3),
                Formula = new List<GradeComponent>
                {
                    new GradeComponent { Name = "Лабораторная 1", Weight = 25, Complexity = 4 },
                    new GradeComponent { Name = "Лабораторная 2", Weight = 25, Complexity = 4 },
                    new GradeComponent { Name = "Тест", Weight = 20, Complexity = 3 },
                    new GradeComponent { Name = "Экзамен", Weight = 30, Complexity = 7 }
                }
            }
        };
    }

    private void InitializeCommands()
    {
        AddSubjectCommand = new RelayCommand(ExecuteAddSubject);
        EditSubjectCommand = new RelayCommand(ExecuteEditSubject, () => CanEditSubject);
        DeleteSubjectCommand = new RelayCommand(ExecuteDeleteSubject, () => CanDeleteSubject);
        AddComponentCommand = new RelayCommand(ExecuteAddComponent, () => CanAddComponent);
        DeleteComponentCommand = new RelayCommand(ExecuteDeleteComponent, () => CanDeleteComponent);
    }

    private async void ExecuteAddSubject()
    {
        var vm = new SubjectDialogViewModel();
        var dialog = new SubjectDialog(vm);

        var owner = GetMainWindow();
        if (owner != null)
        {
            await dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }

        if (vm.Result != null)
        {
            Subjects.Add(vm.Result);
            SelectedSubject = vm.Result;
            StatusMessage = $"Добавлен предмет: {vm.Result.Name}";
        }
    }

    private async void ExecuteEditSubject()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        var vm = new SubjectDialogViewModel();
        vm.LoadSubject(SelectedSubject);
        var dialog = new SubjectDialog(vm);

        var owner = GetMainWindow();
        if (owner != null)
        {
            await dialog.ShowDialog(owner);
        }
        else
        {
            dialog.Show();
        }

        if (vm.Result != null)
        {
            SelectedSubject.Name = vm.Result.Name;
            SelectedSubject.TargetGrade = vm.Result.TargetGrade;
            OnPropertyChanged(nameof(SelectedSubject));
            UpdateStatusMessage();
            StatusMessage = $"Предмет обновлён: {SelectedSubject.Name}";
        }
    }

    private void ExecuteDeleteSubject()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        string subjectName = SelectedSubject.Name;
        Subjects.Remove(SelectedSubject);
        SelectedSubject = null;
        StatusMessage = $"Удалён предмет: {subjectName}";
    }

    private void ExecuteAddComponent()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        var component = new GradeComponent
        {
            Name = $"Компонент {SelectedSubject.Formula.Count + 1}",
            Weight = 10,
            Complexity = 5,
            CurrentGrade = 0
        };

        SelectedSubject.Formula.Add(component);
        SelectedComponent = component;
        RefreshFormulaStats();
        StatusMessage = $"Добавлен компонент: {component.Name}";
    }

    private void ExecuteDeleteComponent()
    {
        if (SelectedSubject == null || SelectedComponent == null)
        {
            return;
        }

        string componentName = SelectedComponent.Name;
        SelectedSubject.Formula.Remove(SelectedComponent);
        SelectedComponent = null;
        RefreshFormulaStats();
        StatusMessage = $"Удалён компонент: {componentName}";
    }

    public void RefreshFormulaStats()
    {
        OnPropertyChanged(nameof(TotalWeightMessage));
        OnPropertyChanged(nameof(IsFormulaValid));
        OnPropertyChanged(nameof(FormulaStatusColor));
    }

    private void UpdateStatusMessage()
    {
        if (SelectedSubject == null)
        {
            StatusMessage = "Предмет не выбран";
            return;
        }

        string formulaInfo = string.Empty;
        if (SelectedSubject.Formula != null && SelectedSubject.Formula.Any())
        {
            formulaInfo = $", {SelectedSubject.Formula.Count} компонентов";
        }

        StatusMessage = $"Выбран: {SelectedSubject.Name}{formulaInfo}";
        RefreshFormulaStats();
    }

    private void RaiseCanExecuteForCommands()
    {
        (EditSubjectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteSubjectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AddComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();

        OnPropertyChanged(nameof(CanEditSubject));
        OnPropertyChanged(nameof(CanDeleteSubject));
        OnPropertyChanged(nameof(CanAddComponent));
        OnPropertyChanged(nameof(CanDeleteComponent));
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }
}