using System.Collections.ObjectModel;
using System.Linq;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.ViewModels;

public class WhatIfViewModel : ViewModelBase
{
    private ObservableCollection<WhatIfComponentViewModel> _components;
    private double _hypotheticalGrade;
    private bool _isBlockingConditionFailed;
    private bool _autoGradeAchieved;
    private string _blockingWarningText;

    public ObservableCollection<WhatIfComponentViewModel> Components
    {
        get => _components;
        set => SetField(ref _components, value);
    }

    public double HypotheticalGrade
    {
        get => _hypotheticalGrade;
        set => SetField(ref _hypotheticalGrade, value);
    }

    public bool IsBlockingConditionFailed
    {
        get => _isBlockingConditionFailed;
        set => SetField(ref _isBlockingConditionFailed, value);
    }

    public bool AutoGradeAchieved
    {
        get => _autoGradeAchieved;
        set => SetField(ref _autoGradeAchieved, value);
    }

    public string HypotheticalGradeText => $"Итоговая оценка: {HypotheticalGrade:F1}";

    public string HypotheticalGradeColor
    {
        get
        {
            if (IsBlockingConditionFailed)
            {
                return "#dc3545";
            }

            return HypotheticalGrade >= 4 ? "#28a745" : "#dc3545";
        }
    }

    public string BlockingWarning
    {
        get => _blockingWarningText;
        private set => SetField(ref _blockingWarningText, value);
    }

    public string AutoGradeStatus
    {
        get
        {
            if (!Components.Any(c => c.IsAutoGrade))
            {
                return string.Empty;
            }

            return AutoGradeAchieved
                ? "Условие автомата выполнено"
                : "Условие автомата не выполнено";
        }
    }

    public string AutoGradeStatusColor => AutoGradeAchieved ? "#28a745" : "#856404";

    public bool HasAutoGradeComponents => Components.Any(c => c.IsAutoGrade);

    public WhatIfViewModel()
    {
        _components = new ObservableCollection<WhatIfComponentViewModel>();
        _blockingWarningText = string.Empty;
    }

    public void LoadFromSubject(Subject subject)
    {
        foreach (WhatIfComponentViewModel old in Components)
        {
            old.SourceComponent.PropertyChanged -= OnSourceComponentPropertyChanged;
            old.GradeChanged -= Recalculate;
        }

        Components.Clear();

        foreach (GradeComponent component in subject.Formula)
        {
            WhatIfComponentViewModel vm = new WhatIfComponentViewModel(component);
            vm.GradeChanged += Recalculate;
            component.PropertyChanged += OnSourceComponentPropertyChanged;
            Components.Add(vm);
        }

        Recalculate();
    }

    private void OnSourceComponentPropertyChanged(
        object sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GradeComponent.IsGraded) ||
            e.PropertyName == nameof(GradeComponent.CurrentGrade) ||
            e.PropertyName == nameof(GradeComponent.IsBlocking) ||
            e.PropertyName == nameof(GradeComponent.BlockingMinimum) ||
            e.PropertyName == nameof(GradeComponent.IsAutoGrade) ||
            e.PropertyName == nameof(GradeComponent.AutoGradeMinScore))
        {
            if (sender is GradeComponent changed)
            {
                WhatIfComponentViewModel? vm = Components
                    .FirstOrDefault(c => c.SourceComponent == changed);

                if (vm != null)
                {
                    vm.SyncFromSource();
                }
            }

            Recalculate();
        }
    }

    private void Recalculate()
    {
        if (Components.Count == 0)
        {
            HypotheticalGrade = 0;
            return;
        }

        double totalWeight = Components.Sum(c => c.Weight);

        if (totalWeight <= 0)
        {
            HypotheticalGrade = 0;
            return;
        }

        double weightedSum = Components.Sum(c => c.HypotheticalGrade * c.Weight);
        HypotheticalGrade = System.Math.Round(weightedSum / totalWeight, 2);

        WhatIfComponentViewModel? failedBlocking = Components
            .FirstOrDefault(c => c.IsBlocking && c.HypotheticalGrade < c.BlockingMinimum);

        IsBlockingConditionFailed = failedBlocking != null;

        BlockingWarning = failedBlocking != null
            ? $"Блокирующий компонент «{failedBlocking.Name}» ниже минимума ({failedBlocking.BlockingMinimum:F1}). Итоговая оценка недостижима."
            : string.Empty;

        AutoGradeAchieved = Components
            .Where(c => c.IsAutoGrade)
            .All(c => c.HypotheticalGrade >= c.AutoGradeMinScore);

        OnPropertyChanged(nameof(HypotheticalGradeText));
        OnPropertyChanged(nameof(HypotheticalGradeColor));
        OnPropertyChanged(nameof(AutoGradeStatus));
        OnPropertyChanged(nameof(AutoGradeStatusColor));
        OnPropertyChanged(nameof(HasAutoGradeComponents));
        OnPropertyChanged(nameof(AutoGradeAchieved));
    }
}