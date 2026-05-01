using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace schedule_automation_app_client.Models;

public class GradeComponent : INotifyPropertyChanged
{
    private Guid _id;
    private string _name;
    private double _weight;
    private int _complexity;
    private double _currentGrade;
    private bool _isGraded;

    public event PropertyChangedEventHandler PropertyChanged;

    public Guid Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public double Weight
    {
        get => _weight;
        set => SetField(ref _weight, value);
    }

    public int Complexity
    {
        get => _complexity;
        set => SetField(ref _complexity, value);
    }

    public double CurrentGrade
    {
        get => _currentGrade;
        set => SetField(ref _currentGrade, value);
    }

    public bool IsGraded
    {
        get => _isGraded;
        set => SetField(ref _isGraded, value);
    }

    public GradeComponent()
    {
        Id = Guid.NewGuid();
        Name = "Новый компонент";
        Weight = 20;
        Complexity = 5;
        CurrentGrade = 0;
        IsGraded = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}