using System.Collections.ObjectModel;
using System.Linq;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.ViewModels;

public class WhatIfViewModel : ViewModelBase
{
    private ObservableCollection<WhatIfComponentViewModel> _components;
    private double _hypotheticalGrade;

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

    public string HypotheticalGradeText => $"Итоговая оценка: {HypotheticalGrade:F1}";

    public string HypotheticalGradeColor => HypotheticalGrade >= 6 ? "#28a745" : "#dc3545";

    public WhatIfViewModel()
    {
        _components = new ObservableCollection<WhatIfComponentViewModel>();
    }

    public void LoadFromSubject(Subject subject)
    {
        Components.Clear();

        foreach (var component in subject.Formula)
        {
            var whatIfComponent = new WhatIfComponentViewModel(component);
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
        HypotheticalGrade = weightedSum / totalWeight;

        OnPropertyChanged(nameof(HypotheticalGradeText));
        OnPropertyChanged(nameof(HypotheticalGradeColor));
    }
}