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

            return HypotheticalGrade >= 6 ? "#28a745" : "#dc3545";
        }
    }

    public string BlockingWarning => IsBlockingConditionFailed
        ? "Блокирующий компонент ниже минимума (4). Итоговая оценка недостижима."
        : string.Empty;

    public string AutoGradeStatus
    {
        get
        {
            if (!Components.Any(c => c.IsAutoGrade))
            {
                return string.Empty;
            }

            return AutoGradeAchieved
                ? "🎓 Условие автомата выполнено"
                : "📚 Условие автомата не выполнено";
        }
    }

    public string AutoGradeStatusColor => AutoGradeAchieved ? "#28a745" : "#856404";

    public bool HasAutoGradeComponents => Components.Any(c => c.IsAutoGrade);

    public WhatIfViewModel()
    {
        _components = new ObservableCollection<WhatIfComponentViewModel>();
    }

    public void LoadFromSubject(Subject subject)
    {
        Components.Clear();

        foreach (GradeComponent component in subject.Formula)
        {
            WhatIfComponentViewModel whatIfComponent = new WhatIfComponentViewModel(component);
            whatIfComponent.GradeChanged += Recalculate;
            Components.Add(whatIfComponent);
        }

        Recalculate();
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

        IsBlockingConditionFailed = Components
            .Any(c => c.IsBlocking && c.HypotheticalGrade < 4.0);

        AutoGradeAchieved = Components
            .Where(c => c.IsAutoGrade)
            .All(c => c.HypotheticalGrade >= c.AutoGradeMinScore);

        OnPropertyChanged(nameof(HypotheticalGradeText));
        OnPropertyChanged(nameof(HypotheticalGradeColor));
        OnPropertyChanged(nameof(BlockingWarning));
        OnPropertyChanged(nameof(AutoGradeStatus));
        OnPropertyChanged(nameof(AutoGradeStatusColor));
        OnPropertyChanged(nameof(HasAutoGradeComponents));
        OnPropertyChanged(nameof(AutoGradeAchieved));
    }
}