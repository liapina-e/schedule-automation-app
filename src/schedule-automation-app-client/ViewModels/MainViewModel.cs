using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;

namespace schedule_automation_app_client.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Subject _selectedSubject;
    private string _statusMessage;
    private ObservableCollection<Subject> _subjects;
    
    public ObservableCollection<Subject> Subjects
    {
        get => _subjects;
        set => SetField(ref _subjects, value);
    }
    
    public Subject SelectedSubject
    {
        get { return _selectedSubject; }
        set
        {
            if (SetField(ref _selectedSubject, value))
            {
                UpdateStatusMessage();
                    
                RaiseCanExecuteForCommands();
            }
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
    
    public ICommand AddSubjectCommand { get; set; }
    public ICommand EditSubjectCommand { get; set; }
    public ICommand DeleteSubjectCommand { get; set; }

    public MainViewModel()
    {
        InitializeTestData();
        
        InitializeCommands();
        
        StatusMessage = "Готово к работе. Выберите предмет или добавьте новый.";
    }

    private void InitializeTestData()
{
    Subjects = new ObservableCollection<Subject>
    {
        new Subject 
        { 
            Id = Guid.NewGuid(),
            Name = "Математический анализ", 
            TargetGrade = 8,
            CreatedDate = DateTime.Now.AddDays(-5),
            Formula = new ObservableCollection<GradeComponent>
            {
                new GradeComponent { Name = "Домашняя работа 1", Weight = 15, Complexity = 3 },
                new GradeComponent { Name = "Домашняя работа 2", Weight = 15, Complexity = 3 },
                new GradeComponent { Name = "Контрольная работа", Weight = 30, Complexity = 5 },
                new GradeComponent { Name = "Экзамен", Weight = 40, Complexity = 8 }
            }
        },
        new Subject 
        { 
            Id = Guid.NewGuid(),
            Name = "Физика", 
            TargetGrade = 7,
            CreatedDate = DateTime.Now.AddDays(-3),
            Formula = new ObservableCollection<GradeComponent>()
            {
                new GradeComponent { Name = "Лабораторная 1", Weight = 25, Complexity = 4 },
                new GradeComponent { Name = "Лабораторная 2", Weight = 25, Complexity = 4 },
                new GradeComponent { Name = "Тест", Weight = 20, Complexity = 3 },
                new GradeComponent { Name = "Экзамен", Weight = 30, Complexity = 7 }
            }
        },
        new Subject 
        { 
            Id = Guid.NewGuid(),
            Name = "Программирование", 
            TargetGrade = 9,
            CreatedDate = DateTime.Now,
            Formula = new ObservableCollection<GradeComponent>
            {
                new GradeComponent { Name = "Проект", Weight = 40, Complexity = 6 },
                new GradeComponent { Name = "Практические задания", Weight = 30, Complexity = 4 },
                new GradeComponent { Name = "Экзамен", Weight = 30, Complexity = 7 }
            }
        }
    };
}

    private void InitializeCommands()
    {
        AddSubjectCommand = new RelayCommand(ExecuteAddSubject);
        EditSubjectCommand = new RelayCommand(ExecuteEditSubject, () => CanEditSubject);
        DeleteSubjectCommand = new RelayCommand(ExecuteDeleteSubject, () => CanDeleteSubject);
    }

    private void ExecuteAddSubject()
    {
        // TODO: Позже здесь будет открытие окна добавления
        Subject newSubject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = $"Новый предмет {Subjects.Count + 1}",
            TargetGrade = 6,
            CreatedDate = DateTime.Now
        };
        
        Subjects.Add(newSubject);
        StatusMessage = $"Добавлен предмет: {newSubject.Name}";
    }

    private void ExecuteEditSubject()
    {
        if (SelectedSubject == null) return;
        
        // TODO: Позже здесь будет открытие окна редактирования
        StatusMessage = $"Редактируется: {SelectedSubject.Name} (пока заглушка)";
    }

    private void ExecuteDeleteSubject()
    {
        if (SelectedSubject == null) return;
        
        Subject subjectToDelete = SelectedSubject;
        var subjectName = subjectToDelete.Name;
        
        Subjects.Remove(subjectToDelete);
        SelectedSubject = null;
        
        StatusMessage = $"Удалён предмет: {subjectName}";
    }

    private void UpdateStatusMessage()
    {
        if (SelectedSubject == null)
        {
            StatusMessage = "Предмет не выбран";
            return;
        }
    
        string formulaInfo = "";
        if (SelectedSubject.Formula != null && SelectedSubject.Formula.Any())
        {
            formulaInfo = $", {SelectedSubject.Formula.Count} компонентов";
        }
    
        StatusMessage = $"Выбран: {SelectedSubject.Name}{formulaInfo}";
    
        OnPropertyChanged(nameof(TotalWeightMessage));
        OnPropertyChanged(nameof(IsFormulaValid));
        OnPropertyChanged(nameof(FormulaStatusColor));
    }

    private void RaiseCanExecuteForCommands()
    {
        if (EditSubjectCommand is RelayCommand editCommand)
        {
            editCommand.RaiseCanExecuteChanged();
        }
        
        if (DeleteSubjectCommand is RelayCommand deleteCommand)
        {
            deleteCommand.RaiseCanExecuteChanged();
        }
        
        OnPropertyChanged(nameof(CanEditSubject));
        OnPropertyChanged(nameof(CanDeleteSubject));
    }
        
}