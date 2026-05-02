using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    private readonly IApiService _apiService;
    private readonly HashSet<Guid> _syncedIds = new HashSet<Guid>();
    private PlanResponseDto? _currentPlan;
    private bool _isLoading;
    private bool _isServerAvailable = false;
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
            if (_selectedSubject?.Formula != null)
            {
                _selectedSubject.Formula.CollectionChanged -= OnFormulaCollectionChanged;
                foreach (var c in _selectedSubject.Formula)
                {
                    c.PropertyChanged -= OnComponentPropertyChanged;
                }
            }

            if (SetField(ref _selectedSubject, value))
            {
                CurrentPlan = null;
                StatusMessage = string.Empty;
                ServerStatus = string.Empty; 

                if (value != null)
                {
                    value.Formula.CollectionChanged += OnFormulaCollectionChanged;
                    foreach (var c in value.Formula)
                    {
                        c.PropertyChanged += OnComponentPropertyChanged;
                    }
                    WhatIf.LoadFromSubject(value);
                    OnPropertyChanged(nameof(WhatIf));
                }

                OnPropertyChanged(nameof(CurrentFormula));
                RefreshFormulaStats();
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
    
    public bool ShowUnachievableWarning =>
        (CurrentPlan != null && !CurrentPlan.IsAchievable) ||
        (!string.IsNullOrEmpty(ServerStatus) && CurrentPlan == null);

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }
    
    public bool IsServerAvailable
    {
        get => _isServerAvailable;
        set
        {
            if (SetField(ref _isServerAvailable, value))
            {
                OnPropertyChanged(nameof(CanCalculatePlan));
                (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string ServerStatus
    {
        get => _serverStatus;
        set
        {
            if (SetField(ref _serverStatus, value))
            {
                OnPropertyChanged(nameof(ShowUnachievableWarning));
            }
        }
    }

    public PlanResponseDto? CurrentPlan
    {
        get => _currentPlan;
        set
        {
            if (SetField(ref _currentPlan, value))
            {
                OnPropertyChanged(nameof(ShowUnachievableWarning));
                OnPropertyChanged(nameof(HasPlansRange));
                OnPropertyChanged(nameof(HasAutoGradePlan));
            }
        }
    }

    public bool HasPlansRange =>
        CurrentPlan != null &&
        CurrentPlan.PlansRange != null &&
        CurrentPlan.PlansRange.Count > 1;

    public bool HasAutoGradePlan =>
        CurrentPlan != null &&
        CurrentPlan.HasAutoGradeOption &&
        CurrentPlan.PlanWithAuto != null;

    public bool CanCalculatePlan => SelectedSubject != null && IsFormulaValid && !IsLoading && IsServerAvailable;

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
        var (loaded, serverAvailable) = await _apiService.LoadSubjectsAsync();

        if (!serverAvailable)
        {
            ServerStatus = "Сервер недоступен при запуске. Перезапустите приложение после запуска сервера.";
            StatusMessage = "Нет подключения к серверу.";
            return;
        }
        
        IsServerAvailable = true;
        ServerStatus = string.Empty;

        if (loaded.Count == 0)
        {
            StatusMessage = "Нет сохранённых предметов. Добавьте первый предмет.";
        }
        else
        {
            Subjects = loaded;
            foreach (Subject s in loaded)
            {
                _syncedIds.Add(s.Id);
            }

            StatusMessage = $"Загружено предметов: {loaded.Count}";
        }
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

        if (!_syncedIds.Contains(SelectedSubject.Id))
        {
            var (created, createError) = await _apiService.CreateSubjectAsync(SelectedSubject);

            if (createError == ApiError.ValidationFailed)
            {
                IsLoading = false;
                ServerStatus = "Некорректные данные в формуле. Проверьте значения оценок (0–10), сложности (1–10) и минимума для автомата (0–10).";
                StatusMessage = "Ошибка в данных формулы.";
                (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanCalculatePlan));
                return;
            }

            if (createError == ApiError.ServerUnavailable || created == null)
            {
                IsLoading = false;
                ServerStatus = "Сервер недоступен. Убедитесь что сервер запущен на localhost:5284.";
                StatusMessage = "Не удалось сохранить предмет на сервере.";
                (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanCalculatePlan));
                return;
            }

            SelectedSubject.Id = created.Id;
            _syncedIds.Add(SelectedSubject.Id);
        }
        else
        {
            var (_, updateError) = await _apiService.UpdateSubjectAsync(SelectedSubject);

            if (updateError == ApiError.ValidationFailed)
            {
                IsLoading = false;
                ServerStatus = "Некорректные данные в формуле. Проверьте значения оценок (0–10), сложности (1–10) и минимума для автомата (0–10).";
                StatusMessage = "Ошибка в данных формулы.";
                (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanCalculatePlan));
                return;
            }

            if (updateError == ApiError.ServerUnavailable)
            {
                IsLoading = false;
                ServerStatus = "Сервер недоступен. Убедитесь что сервер запущен на localhost:5284.";
                StatusMessage = "Не удалось получить план — сервер недоступен.";
                (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanCalculatePlan));
                return;
            }
        }

        PlanResponseDto? plan = await _apiService.CalculatePlanAsync(SelectedSubject.Id);

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

        if (vm.Result == null)
        {
            return;
        }

        Subjects.Add(vm.Result);
        SelectedSubject = vm.Result;
        StatusMessage = $"Добавлен предмет: {vm.Result.Name}. Настройте формулу и нажмите «Рассчитать план».";
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

        if (vm.Result == null)
        {
            return;
        }

        SelectedSubject.Name = vm.Result.Name;
        SelectedSubject.TargetGrade = vm.Result.TargetGrade;
        SelectedSubject.HasAutoGrade = vm.Result.HasAutoGrade;
        SelectedSubject.AutoGradeMinScore = vm.Result.AutoGradeMinScore;

        if (!_syncedIds.Contains(SelectedSubject.Id))
        {
            OnPropertyChanged(nameof(SelectedSubject));
            UpdateStatusMessage();
            StatusMessage = $"Предмет обновлён: {SelectedSubject.Name}";
            return;
        }

        IsLoading = true;
        StatusMessage = "Обновляем предмет...";

        var (_, error) = await _apiService.UpdateSubjectAsync(SelectedSubject);

        IsLoading = false;

        if (error == ApiError.ServerUnavailable)
        {
            ServerStatus = "Не удалось обновить предмет — сервер недоступен.";
            StatusMessage = "Ошибка при обновлении.";
            return;
        }

        OnPropertyChanged(nameof(SelectedSubject));
        UpdateStatusMessage();
        StatusMessage = $"Предмет обновлён: {SelectedSubject.Name}";
    }

    private async void ExecuteDeleteSubject()
    {
        if (SelectedSubject == null)
        {
            return;
        }

        string subjectName = SelectedSubject.Name;
        Guid subjectId = SelectedSubject.Id;
        bool wasOnServer = _syncedIds.Contains(subjectId);

        Subjects.Remove(SelectedSubject);
        _syncedIds.Remove(subjectId);
        SelectedSubject = null;

        if (wasOnServer)
        {
            bool deleted = await _apiService.DeleteSubjectAsync(subjectId);

            if (!deleted)
            {
                ServerStatus = "Не удалось удалить предмет на сервере.";
            }
        }

        StatusMessage = $"Удалён предмет: {subjectName}";
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
    }

    public void RefreshFormulaStats()
    {
        OnPropertyChanged(nameof(TotalWeightMessage));
        OnPropertyChanged(nameof(IsFormulaValid));
        OnPropertyChanged(nameof(FormulaStatusColor));
        OnPropertyChanged(nameof(CanCalculatePlan));
        OnPropertyChanged(nameof(CanSelectBlockingMinimum));
        (CalculatePlanCommand as RelayCommand)?.RaiseCanExecuteChanged();
        UpdateStatusMessage();
    }
    
    private void OnFormulaCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (GradeComponent c in e.NewItems)
                c.PropertyChanged += OnComponentPropertyChanged;
        }
        if (e.OldItems != null)
        {
            foreach (GradeComponent c in e.OldItems)
                c.PropertyChanged -= OnComponentPropertyChanged;
        }
    
        RefreshFormulaStats();
        if (SelectedSubject != null)
            WhatIf.LoadFromSubject(SelectedSubject);
    }
    
    private void OnComponentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        RefreshFormulaStats();
    }

    private async Task SyncSubjectAsync()
    {
        if (SelectedSubject == null || !IsFormulaValid || !_syncedIds.Contains(SelectedSubject.Id))
        {
            return;
        }

        await _apiService.UpdateSubjectAsync(SelectedSubject);
    }

    private void UpdateStatusMessage()
    {
        if (SelectedSubject == null)
        {
            StatusMessage = "Выберите предмет или добавьте новый";
            return;
        }

        if (SelectedSubject.Formula.Count == 0)
        {
            StatusMessage = "Добавьте компоненты формулы во вкладке «Формула»";
            return;
        }

        GradeComponent invalidComponent = SelectedSubject.Formula.FirstOrDefault(c =>
            c.CurrentGrade < 0 || c.CurrentGrade > 10 ||
            c.Complexity < 1 || c.Complexity > 10 ||
            c.Weight < 0 || c.Weight > 100);

        if (invalidComponent != null)
        {
            StatusMessage = $"Некорректные значения у компонента «{invalidComponent.Name}». " +
                            "Проверьте оценку (0–10), сложность (1–10) и вес (0–100).";
            return;
        }

        double totalWeight = SelectedSubject.Formula.Sum(c => c.Weight);
        if (totalWeight != 100)
        {
            StatusMessage = $"Сумма весов: {totalWeight} из 100. " +
                            (totalWeight < 100
                                ? $"Добавьте ещё {100 - totalWeight}%."
                                : $"Превышение на {totalWeight - 100}%.");
            return;
        }

        StatusMessage = "Формула готова, нажмите «Рассчитать план»";
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