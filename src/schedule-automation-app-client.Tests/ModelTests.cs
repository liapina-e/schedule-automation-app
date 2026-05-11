using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;

namespace schedule_automation_app_client.Tests;

public class ModelTests
{
    [AvaloniaFact]
    public void GradeComponent_DefaultConstructor_SetsDefaults()
    {
        GradeComponent c = new GradeComponent();

        Assert.Equal("Новый компонент", c.Name);
        Assert.Equal(20, c.Weight);
        Assert.Equal(5, c.Complexity);
        Assert.Equal(0, c.CurrentGrade);
        Assert.False(c.IsGraded);
        Assert.False(c.IsBlocking);
        Assert.False(c.IsAutoGrade);
        Assert.Equal(0, c.AutoGradeMinScore);
        Assert.Equal(4.0, c.BlockingMinimum);
        Assert.NotEqual(Guid.Empty, c.Id);
    }

    [AvaloniaFact]
    public void GradeComponent_PropertyChanged_FiredWhenNameChanges()
    {
        GradeComponent c = new GradeComponent();
        bool fired = false;
        c.PropertyChanged += (_, _) => fired = true;

        c.Name = "Экзамен";

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void GradeComponent_PropertyChanged_NotFiredWhenSameValue()
    {
        GradeComponent c = new GradeComponent();
        c.Name = "Экзамен";
        int count = 0;
        c.PropertyChanged += (_, _) => count++;

        c.Name = "Экзамен";

        Assert.Equal(0, count);
    }

    [AvaloniaFact]
    public void GradeComponent_Weight_CanBeSetToZero()
    {
        GradeComponent c = new GradeComponent();
        c.Weight = 0;
        Assert.Equal(0, c.Weight);
    }

    [AvaloniaFact]
    public void GradeComponent_Weight_CanBeSetTo100()
    {
        GradeComponent c = new GradeComponent();
        c.Weight = 100;
        Assert.Equal(100, c.Weight);
    }

    [AvaloniaFact]
    public void GradeComponent_IsBlocking_PropertyChangedFired()
    {
        GradeComponent c = new GradeComponent();
        string? changedProp = null;
        c.PropertyChanged += (_, e) => changedProp = e.PropertyName;

        c.IsBlocking = true;

        Assert.Equal(nameof(GradeComponent.IsBlocking), changedProp);
    }

    [AvaloniaFact]
    public void GradeComponent_BlockingMinimum_PropertyChangedFired()
    {
        GradeComponent c = new GradeComponent();
        bool fired = false;
        c.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(GradeComponent.BlockingMinimum)) fired = true;
        };

        c.BlockingMinimum = 3.5;

        Assert.True(fired);
        Assert.Equal(3.5, c.BlockingMinimum);
    }

    [AvaloniaFact]
    public void GradeComponent_AutoGradeMinScore_CanBeSet()
    {
        GradeComponent c = new GradeComponent();
        c.AutoGradeMinScore = 7.5;
        Assert.Equal(7.5, c.AutoGradeMinScore);
    }

    [AvaloniaFact]
    public void GradeComponent_IsAutoGrade_CanBeSetToTrue()
    {
        GradeComponent c = new GradeComponent();
        c.IsAutoGrade = true;
        Assert.True(c.IsAutoGrade);
    }

    [AvaloniaFact]
    public void GradeComponent_IsGraded_CanBeSetToTrue()
    {
        GradeComponent c = new GradeComponent();
        c.IsGraded = true;
        Assert.True(c.IsGraded);
    }

    [AvaloniaFact]
    public void GradeComponent_CurrentGrade_CanBeSetToMax()
    {
        GradeComponent c = new GradeComponent();
        c.CurrentGrade = 10;
        Assert.Equal(10, c.CurrentGrade);
    }
    
    [AvaloniaFact]
    public void Subject_DefaultConstructor_SetsDefaults()
    {
        Subject s = new Subject();

        Assert.Equal("Новый предмет", s.Name);
        Assert.Equal(4, s.TargetGrade);
        Assert.NotNull(s.Formula);
        Assert.Empty(s.Formula);
        Assert.False(s.HasAutoGrade);
        Assert.Equal(8, s.AutoGradeMinScore);
        Assert.NotEqual(Guid.Empty, s.Id);
    }

    [AvaloniaFact]
    public void Subject_TargetGrade_ThrowsWhenBelow4()
    {
        Subject s = new Subject();
        Assert.Throws<ArgumentOutOfRangeException>(() => s.TargetGrade = 3);
    }

    [AvaloniaFact]
    public void Subject_TargetGrade_ThrowsWhenAbove10()
    {
        Subject s = new Subject();
        Assert.Throws<ArgumentOutOfRangeException>(() => s.TargetGrade = 11);
    }

    [AvaloniaFact]
    public void Subject_TargetGrade_CanBeSet4()
    {
        Subject s = new Subject();
        s.TargetGrade = 4;
        Assert.Equal(4, s.TargetGrade);
    }

    [AvaloniaFact]
    public void Subject_TargetGrade_CanBeSet10()
    {
        Subject s = new Subject();
        s.TargetGrade = 10;
        Assert.Equal(10, s.TargetGrade);
    }

    [AvaloniaFact]
    public void Subject_DisplayInfo_ContainsNameAndGrade()
    {
        Subject s = new Subject { Name = "Математика" };
        s.TargetGrade = 7;

        Assert.Contains("Математика", s.DisplayInfo);
        Assert.Contains("7", s.DisplayInfo);
    }

    [AvaloniaFact]
    public void Subject_Name_PropertyChangedFired()
    {
        Subject s = new Subject();
        bool fired = false;
        s.PropertyChanged += (_, _) => fired = true;

        s.Name = "Физика";

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void Subject_Name_PropertyChangedIncludesDisplayInfo()
    {
        Subject s = new Subject();
        bool displayInfoFired = false;
        s.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Subject.DisplayInfo)) displayInfoFired = true;
        };

        s.Name = "Химия";

        Assert.True(displayInfoFired);
    }

    [AvaloniaFact]
    public void Subject_TargetGrade_PropertyChangedFired()
    {
        Subject s = new Subject();
        bool fired = false;
        s.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Subject.TargetGrade)) fired = true;
        };

        s.TargetGrade = 8;

        Assert.True(fired);
    }

    [AvaloniaFact]
    public void Subject_HasAutoGrade_CanBeSetToTrue()
    {
        Subject s = new Subject();
        s.HasAutoGrade = true;
        Assert.True(s.HasAutoGrade);
    }

    [AvaloniaFact]
    public void Subject_AutoGradeMinScore_CanBeSet()
    {
        Subject s = new Subject();
        s.AutoGradeMinScore = 9.0;
        Assert.Equal(9.0, s.AutoGradeMinScore);
    }

    [AvaloniaFact]
    public void Subject_UpdatedAt_ChangesWhenNameChanges()
    {
        Subject s = new Subject();
        DateTime before = s.UpdatedAt;

        System.Threading.Thread.Sleep(10);
        s.Name = "Новое имя";

        Assert.True(s.UpdatedAt >= before);
    }
    
    [AvaloniaFact]
    public void RelayCommand_Execute_CallsAction()
    {
        bool called = false;
        RelayCommand cmd = new RelayCommand(() => called = true);

        cmd.Execute(null);

        Assert.True(called);
    }

    [AvaloniaFact]
    public void RelayCommand_CanExecute_TrueWhenNoPredicate()
    {
        RelayCommand cmd = new RelayCommand(() => { });
        Assert.True(cmd.CanExecute(null));
    }

    [AvaloniaFact]
    public void RelayCommand_CanExecute_FalseWhenPredicateReturnsFalse()
    {
        RelayCommand cmd = new RelayCommand(() => { }, () => false);
        Assert.False(cmd.CanExecute(null));
    }

    [AvaloniaFact]
    public void RelayCommand_CanExecute_TrueWhenPredicateReturnsTrue()
    {
        RelayCommand cmd = new RelayCommand(() => { }, () => true);
        Assert.True(cmd.CanExecute(null));
    }

    [AvaloniaFact]
    public void RelayCommand_RaiseCanExecuteChanged_FiredEvent()
    {
        RelayCommand cmd = new RelayCommand(() => { });
        bool fired = false;
        cmd.CanExecuteChanged += (_, _) => fired = true;

        cmd.RaiseCanExecuteChanged();

        Assert.True(fired);
    }
}