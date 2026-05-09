using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Domain.Exceptions;
using Xunit;

namespace schedule_automation_app_server.Tests;

public class DomainEntityTests
{

    [Fact]
    public void Subject_EmptyName_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Subject("", 7));
    }

    [Fact]
    public void Subject_WhitespaceName_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Subject("   ", 7));
    }

    [Fact]
    public void Subject_TargetGradeBelow4_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Subject("Тест", 3));
    }

    [Fact]
    public void Subject_TargetGradeAbove10_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => new Subject("Тест", 11));
    }

    [Fact]
    public void Subject_TargetGrade4_CreatesSuccessfully()
    {
        Subject subject = new Subject("Тест", 4);
        Assert.Equal(4, subject.TargetGrade);
    }

    [Fact]
    public void Subject_TargetGrade10_CreatesSuccessfully()
    {
        Subject subject = new Subject("Тест", 10);
        Assert.Equal(10, subject.TargetGrade);
    }

    [Fact]
    public void Subject_AddNullComponent_ThrowsArgumentNullException()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Throws<ArgumentNullException>(() => subject.AddComponent(null!));
    }

    [Fact]
    public void Subject_Update_ChangesNameAndGrade()
    {
        Subject subject = new Subject("Старое", 6);
        subject.Update("Новое", 8);

        Assert.Equal("Новое", subject.Name);
        Assert.Equal(8, subject.TargetGrade);
    }

    [Fact]
    public void Subject_UpdateWithInvalidGrade_ThrowsDomainValidationException()
    {
        Subject subject = new Subject("Тест", 7);
        Assert.Throws<DomainValidationException>(() => subject.Update("Тест", 3));
    }
    
    [Fact]
    public void GradeComponent_EmptyName_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("", 50, 5, 5));
    }

    [Fact]
    public void GradeComponent_WeightAbove100_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("А", 101, 5, 5));
    }

    [Fact]
    public void GradeComponent_ComplexityBelow1_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("А", 50, 0, 5));
    }

    [Fact]
    public void GradeComponent_ComplexityAbove10_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("А", 50, 11, 5));
    }

    [Fact]
    public void GradeComponent_GradeBelow0_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("А", 50, 5, -1));
    }

    [Fact]
    public void GradeComponent_GradeAbove10_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            new GradeComponent("А", 50, 5, 11));
    }

    [Fact]
    public void GradeComponent_GradeExactly0_CreatesSuccessfully()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 0);
        Assert.Equal(0, c.CurrentGrade);
    }

    [Fact]
    public void GradeComponent_GradeExactly10_CreatesSuccessfully()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 10);
        Assert.Equal(10, c.CurrentGrade);
    }

    [Fact]
    public void GradeComponent_CanBeImproved_WhenNotGradedAndBelow10()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 5, false);
        Assert.True(c.CanBeImproved());
    }

    [Fact]
    public void GradeComponent_CannotBeImproved_WhenGraded()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 5, true);
        Assert.False(c.CanBeImproved());
    }

    [Fact]
    public void GradeComponent_CannotBeImproved_WhenGradeIs10()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 10, false);
        Assert.False(c.CanBeImproved());
    }

    [Fact]
    public void GradeComponent_IsBlockingConditionMet_WhenAboveMinimum()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 5)
        {
            IsBlocking = true,
            MinimumGrade = 4.0
        };
        Assert.True(c.IsBlockingConditionMet());
    }

    [Fact]
    public void GradeComponent_IsBlockingConditionNotMet_WhenBelowMinimum()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 3)
        {
            IsBlocking = true,
            MinimumGrade = 4.0
        };
        Assert.False(c.IsBlockingConditionMet());
    }

    [Fact]
    public void GradeComponent_WeightAsDecimal_Returns50PercentAs0Point5()
    {
        GradeComponent c = new GradeComponent("А", 50, 5, 5);
        Assert.Equal(0.5, c.WeightAsDecimal(), precision: 3);
    }
}