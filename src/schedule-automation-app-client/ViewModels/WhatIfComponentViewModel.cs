using System;
using schedule_automation_app_client.Models;

namespace schedule_automation_app_client.ViewModels;

public class WhatIfComponentViewModel : ViewModelBase
{
    private double _hypotheticalGrade;
    private bool _isGraded;
    private bool _isBlocking;
    private bool _isAutoGrade;
    private double _autoGradeMinScore;

    public GradeComponent SourceComponent { get; }

    public string Name { get; }
    public double Weight { get; }
    public double CurrentGrade => SourceComponent.CurrentGrade;

    public bool IsGraded
    {
        get => _isGraded;
        private set => SetField(ref _isGraded, value);
    }

    public bool IsBlocking
    {
        get => _isBlocking;
        private set => SetField(ref _isBlocking, value);
    }

    public bool IsAutoGrade
    {
        get => _isAutoGrade;
        private set => SetField(ref _isAutoGrade, value);
    }

    public double AutoGradeMinScore
    {
        get => _autoGradeMinScore;
        private set => SetField(ref _autoGradeMinScore, value);
    }

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
        SourceComponent = component;
        Name = component.Name;
        Weight = component.Weight;
        _isGraded = component.IsGraded;
        _isBlocking = component.IsBlocking;
        _isAutoGrade = component.IsAutoGrade;
        _autoGradeMinScore = component.AutoGradeMinScore;
        _hypotheticalGrade = component.CurrentGrade;
    }

    public void SyncFromSource()
    {
        IsGraded = SourceComponent.IsGraded;
        IsBlocking = SourceComponent.IsBlocking;
        IsAutoGrade = SourceComponent.IsAutoGrade;
        AutoGradeMinScore = SourceComponent.AutoGradeMinScore;
        OnPropertyChanged(nameof(CurrentGrade));

        if (IsGraded)
        {
            SetField(ref _hypotheticalGrade, SourceComponent.CurrentGrade, nameof(HypotheticalGrade));
        }
    }
}