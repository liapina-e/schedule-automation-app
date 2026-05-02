using System;
using System.Windows.Input;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;

namespace schedule_automation_app_client.ViewModels;

public class SubjectDialogViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private decimal? _targetGradeValue = 6;
    private string _nameError = string.Empty;
    private string _gradeError = string.Empty;
    private bool _isEditMode;
    private bool _hasAutoGrade;
    private decimal? _autoGradeMinScoreValue = 8;

    public string Name
    {
        get => _name;
        set
        {
            SetField(ref _name, value);
            ValidateName();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public decimal? TargetGradeValue
    {
        get => _targetGradeValue;
        set
        {
            SetField(ref _targetGradeValue, value);
            ValidateGrade();
            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public bool HasAutoGrade
    {
        get => _hasAutoGrade;
        set => SetField(ref _hasAutoGrade, value);
    }

    public decimal? AutoGradeMinScoreValue
    {
        get => _autoGradeMinScoreValue;
        set => SetField(ref _autoGradeMinScoreValue, value);
    }

    public string NameError
    {
        get => _nameError;
        set => SetField(ref _nameError, value);
    }

    public string GradeError
    {
        get => _gradeError;
        set => SetField(ref _gradeError, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            SetField(ref _isEditMode, value);
            OnPropertyChanged(nameof(Title));
        }
    }

    public string Title => IsEditMode ? "Редактировать предмет" : "Добавить предмет";

    public Subject? Result { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? CloseRequested;

    public SubjectDialogViewModel()
    {
        SaveCommand = new RelayCommand(ExecuteSave, CanSave);
        CancelCommand = new RelayCommand(ExecuteCancel);
    }

    public void LoadSubject(Subject subject)
    {
        IsEditMode = true;
        Name = subject.Name;
        TargetGradeValue = (decimal)subject.TargetGrade;
        HasAutoGrade = subject.HasAutoGrade;
        AutoGradeMinScoreValue = (decimal)subject.AutoGradeMinScore;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrEmpty(NameError)
               && string.IsNullOrEmpty(GradeError)
               && TargetGradeValue.HasValue;
    }

    private void ExecuteSave()
    {
        if (!CanSave())
        {
            return;
        }

        Result = new Subject
        {
            Name = Name.Trim(),
            TargetGrade = (int)TargetGradeValue!.Value,
            HasAutoGrade = HasAutoGrade,
            AutoGradeMinScore = HasAutoGrade && AutoGradeMinScoreValue.HasValue
                ? (double)AutoGradeMinScoreValue.Value
                : 0
        };

        CloseRequested?.Invoke(true);
    }

    private void ExecuteCancel()
    {
        Result = null;
        CloseRequested?.Invoke(false);
    }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Название не может быть пустым";
        }
        else if (Name.Trim().Length > 100)
        {
            NameError = "Название не может быть длиннее 100 символов";
        }
        else
        {
            NameError = string.Empty;
        }
    }

    private void ValidateGrade()
    {
        if (!TargetGradeValue.HasValue)
        {
            GradeError = "Введите оценку";
        }
        else if (TargetGradeValue.Value < 4)
        {
            GradeError = "Минимальная оценка — 4";
        }
        else if (TargetGradeValue.Value > 10)
        {
            GradeError = "Максимальная оценка — 10";
        }
        else
        {
            GradeError = string.Empty;
        }
    }
}