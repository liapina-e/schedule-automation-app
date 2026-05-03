using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;
using schedule_automation_app_client.Services.Dtos;
using schedule_automation_app_client.Views;

namespace schedule_automation_app_client.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Subject _selectedSubject;
    private string _statusMessage;
    private ObservableCollection<Subject> _subjects;
    private GradeComponent _selectedComponent;
    private readonly IStorageService _storageService;
    private readonly IApiService _apiService;
    private PlanResponseDto? _currentPlan;
    private bool _isLoading;
    private string _serverStatus;
    private WhatIfViewModel _whatIf;

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
                CurrentPlan = null;

                if (value != null)
                {
                    WhatIf.LoadFromSubject(value);
                }

                OnPropertyChanged(nameof(CurrentFormula));
                OnPropertyChanged(nameof(CanCalculatePlan));
                UpdateStatusMessage();
                RaiseCanExecuteForCommands();
            }
        }
    }

    public ObservableCollection<GradeComponent> CurrentFormula
    {
        get
        {
            if (SelectedSubject == null)
            {
                return null;
            }

            return SelectedSubject.Formula;
        }
    }

    public GradeComponent SelectedComponent
    {
        get => _selectedComponent;
        set
        {
            if (_selectedComponent != null)
            {
                _selectedComponent.PropertyChanged -= OnSelectedComponentPropertyChanged;
            }

            if (SetField(ref _selectedComponent, value))
            {
                if (_selectedComponent != null)
                {
                    _selectedComponent.PropertyChanged += OnSelectedComponentPropertyChanged;
                }

                (DeleteComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanDeleteComponent));
                OnPropertyChanged(nameof(CanSelectBlockingMinimum));
            }
        }
    }

    private void OnSelectedComponentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GradeComponent.IsBlocking))
        {
            OnPropertyChanged(nameof(CanSelectBlockingMinimum));
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

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public string ServerStatus
    {
        get => _serverStatus;
        set => SetField(ref _serverStatus, value);
    }

    public PlanResponseDto? CurrentPlan
    {
        get => _currentPlan;
        set => SetField(ref _currentPlan, value);
    }
    
    public bool HasPlansRange =>
        CurrentPlan != null &&
        CurrentPlan.PlansRange != null &&
        CurrentPlan.PlansRange.Count > 1;

    public bool HasAutoGradePlan =>
        CurrentPlan != null &&
        CurrentPlan.HasAutoGradeOption &&
        CurrentPlan.PlanWithAuto != null;

    public bool CanCalculatePlan => SelectedSubject != null && IsFormulaValid && !IsLoading;

    public WhatIfViewModel WhatIf
    {
        get => _whatIf;
        set => SetField(ref _whatIf, value);
    }
    
    public bool CanSelectBlockingMinimum =>
        SelectedComponent != null && SelectedComponent.IsBlocking;

    public double[] BlockingMinimumOptions => new[] { 3.5, 4.0 };

    public ICommand AddSubjectCommand { get; set; }
    public ICommand EditSubjectCommand { get; set; }
    public ICommand DeleteSubjectCommand { get; set; }
    public ICommand AddComponentCommand { get; set; }
    public ICommand DeleteComponentCommand { get; set; }
    public ICommand CalculatePlanCommand { get; set; }

    public MainViewModel()
    {
        _storageService = new JsonStorageService();
        _apiService = new ApiService();
        _whatIf = new WhatIfViewModel();
        WhatIf = _whatIf;

        Subjects = new ObservableCollection<Subject>();

        InitializeCommands();

        StatusMessage = "Загрузка данных...";

        LoadSubjectsAsync();
    }

    private async void LoadSubjectsAsync()
    {
        ObservableCollection<Subject> loaded = await _storageService.LoadAsync();

        if (loaded.Count == 0)
        {
            StatusMessage = "Нет сохранённых предметов. Добавьте первый предмет.";
        }
        else
        {
            Subjects = loaded;
            StatusMessage = $"Загружено предметов: {loaded.Count}";
        }
    }

    private async void SaveSubjectsAsync()
    {
        await _storageService.SaveAsync(Subjects);
    }

    private void InitializeCommands()
    {
        AddSubjectCommand = new RelayCommand(ExecuteAddSubject);
        EditSubjectCommand = new RelayCommand(ExecuteEditSubject, () => CanEditSubject);
        DeleteSubjectCommand = new RelayCommand(ExecuteDeleteSubject, () => CanDeleteSubject);
        AddComponentCommand = new RelayCommand(ExecuteAddComponent, () => CanAddComponent);
        DeleteComponentCommand = new RelayCommand(ExecuteDeleteComponent, () => CanDeleteComponent);
        CalculatePlanCommand = new RelayCommand(ExecuteCalculatePlan, () => CanCalculatePlan);
    }

    private async void ExecuteCalculatePlan()
    {
        if (SelectedSubject == null || !IsFormulaValid)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Считаем план...";
        (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(CanCalculatePlan));

        PlanResponseDto? plan = await _apiService.CalculatePlanAsync(SelectedSubject);

        IsLoading = false;

        if (plan == null)
        {
            ServerStatus = "Сервер недоступен. Убедитесь что сервер запущен на localhost:5284.";
            CurrentPlan = null;
            StatusMessage = "Не удалось получить план — сервер недоступен.";
        }
        else if (!plan.IsAchievable)
        {
            CurrentPlan = plan;
            OnPropertyChanged(nameof(HasPlansRange));
            OnPropertyChanged(nameof(HasAutoGradePlan));
            ServerStatus = plan.Recommendation;
            StatusMessage = "Цель недостижима с текущими настройками.";
        }
        else if (plan.OptimalPlan.Count == 0)
        {
            CurrentPlan = plan;
            OnPropertyChanged(nameof(HasPlansRange));
            OnPropertyChanged(nameof(HasAutoGradePlan));
            ServerStatus = "Текущих оценок уже достаточно для достижения цели.";
            StatusMessage = $"Цель уже достигнута! Текущая оценка: {plan.CurrentGrade:F2}";
        }
        else
        {
            CurrentPlan = plan;
            OnPropertyChanged(nameof(HasPlansRange));
            OnPropertyChanged(nameof(HasAutoGradePlan));
            ServerStatus = $"План рассчитан. Текущая оценка: {plan.CurrentGrade:F2}";
            StatusMessage = $"План для предмета \"{SelectedSubject.Name}\" готов";
        }

        (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(CanCalculatePlan));
    }

    private async void ExecuteAddSubject()
    {
        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        SubjectDialog dialog = new SubjectDialog(vm);

        Window? owner = GetMainWindow();
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
            SaveSubjectsAsync();
        }
    }

    private async void ExecuteEditSubject()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        SubjectDialogViewModel vm = new SubjectDialogViewModel();
        vm.LoadSubject(SelectedSubject);
        SubjectDialog dialog = new SubjectDialog(vm);

        Window? owner = GetMainWindow();
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
            SelectedSubject.HasAutoGrade = vm.Result.HasAutoGrade;
            SelectedSubject.AutoGradeMinScore = vm.Result.AutoGradeMinScore;
            OnPropertyChanged(nameof(SelectedSubject));
            UpdateStatusMessage();
            StatusMessage = $"Предмет обновлён: {SelectedSubject.Name}";
            SaveSubjectsAsync();
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
        SaveSubjectsAsync();
    }

    private void ExecuteAddComponent()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        GradeComponent component = new GradeComponent
        {
            Name = $"Компонент {SelectedSubject.Formula.Count + 1}",
            Weight = 10,
            Complexity = 5,
            CurrentGrade = 0,
            IsGraded = false
        };

        SelectedSubject.Formula.Add(component);
        SelectedComponent = component;
        OnPropertyChanged(nameof(CurrentFormula));
        RefreshFormulaStats();
        StatusMessage = $"Добавлен компонент: {component.Name}";
        SaveSubjectsAsync();
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
        OnPropertyChanged(nameof(CurrentFormula));
        RefreshFormulaStats();
        StatusMessage = $"Удалён компонент: {componentName}";
        SaveSubjectsAsync();
    }

    public void RefreshFormulaStats()
    {
        OnPropertyChanged(nameof(TotalWeightMessage));
        OnPropertyChanged(nameof(IsFormulaValid));
        OnPropertyChanged(nameof(FormulaStatusColor));
        OnPropertyChanged(nameof(CanCalculatePlan));
        OnPropertyChanged(nameof(CanSelectBlockingMinimum));
        (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
        SaveSubjectsAsync();
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

        string autoInfo = string.Empty;
        if (SelectedSubject.HasAutoGrade)
        {
            double currentGrade = SelectedSubject.Formula != null && SelectedSubject.Formula.Any()
                ? SelectedSubject.Formula.Sum(c => c.CurrentGrade * c.Weight / 100.0)
                : 0;

            autoInfo = currentGrade >= SelectedSubject.AutoGradeMinScore
                ? " | Автомат: ✓"
                : $" | Автомат: нужно {SelectedSubject.AutoGradeMinScore:F1}";
        }

        StatusMessage = $"Выбран: {SelectedSubject.Name}{formulaInfo}{autoInfo}";
        RefreshFormulaStats();
    }

    private void RaiseCanExecuteForCommands()
    {
        (EditSubjectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteSubjectCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AddComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (DeleteComponentCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();

        OnPropertyChanged(nameof(CanEditSubject));
        OnPropertyChanged(nameof(CanDeleteSubject));
        OnPropertyChanged(nameof(CanAddComponent));
        OnPropertyChanged(nameof(CanDeleteComponent));
        OnPropertyChanged(nameof(CanCalculatePlan));
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