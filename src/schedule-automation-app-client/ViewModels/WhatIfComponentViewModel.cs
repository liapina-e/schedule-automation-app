using System;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.ViewModels;

public class WhatIfComponentViewModel : ViewModelBase
{
    private double _hypotheticalGrade;

    public string Name { get; }
    public double Weight { get; }
    public double CurrentGrade { get; }
    public bool IsGraded { get; }

    public double HypotheticalGrade
    {
        get => _hypotheticalGrade;
        set
        {
            if (IsGraded)
            {
                return;
            }

            if (value < 0)
            {
                value = 0;
            }

            if (value > 10)
            {
                value = 10;
            }

            if (SetField(ref _hypotheticalGrade, value))
            {
                GradeChanged?.Invoke();
            }
        }
    }

    public event Action? GradeChanged;

    public WhatIfComponentViewModel(GradeComponent component)
    {
        Name = component.Name;
        Weight = component.Weight;
        CurrentGrade = component.CurrentGrade;
        IsGraded = component.IsGraded;
        _hypotheticalGrade = component.CurrentGrade;
    }
}