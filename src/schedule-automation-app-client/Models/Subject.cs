using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace schedule_automation_app_client.Models;

public class Subject : INotifyPropertyChanged
{
    private Guid _id;
    private string _name;
    private int _targetGrade;
    private List<GradeComponent> _formula;
    private DateTime _createdAt;
    private DateTime _updatedAt;
    
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
    
    public int TargetGrade
    {
        get => _targetGrade;
        set
        {
            if (value < 4 || value > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetGrade), "Оценка должна быть от 4 до 10");

            }
                
            SetField(ref _targetGrade, value);
        }
    }

    public List<GradeComponent> Formula
    {
        get => _formula;
        set => SetField(ref _formula, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetField(ref _createdAt, value);
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set => SetField(ref _updatedAt, value);
    }

    public Subject()
    {
        Id = Guid.NewGuid();
        Name = "Новый предмет";
        TargetGrade = 4; // мин оценка по умолчанию
        Formula = new List<GradeComponent>();
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
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
        if (propertyName != nameof(UpdatedAt))
        {
            UpdatedAt = DateTime.Now;
        }
                
        return true;
    }

    public string DisplayInfo => $"{Name} (Цель: {TargetGrade})";
}