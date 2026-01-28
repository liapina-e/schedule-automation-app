using System;
using System.Collections.ObjectModel;
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
                CreatedAt = DateTime.Now.AddDays(-5)
            },
            new Subject 
            { 
                Id = Guid.NewGuid(),
                Name = "Физика", 
                TargetGrade = 7,
                CreatedAt = DateTime.Now.AddDays(-3)
            },
            new Subject 
            { 
                Id = Guid.NewGuid(),
                Name = "Программирование", 
                TargetGrade = 9,
                CreatedAt = DateTime.Now
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
            CreatedAt = DateTime.Now
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
        StatusMessage = SelectedSubject != null 
            ? $"Выбран: {SelectedSubject.Name}" 
            : "Предмет не выбран";
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