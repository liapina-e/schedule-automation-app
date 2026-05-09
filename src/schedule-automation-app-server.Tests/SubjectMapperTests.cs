using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Mappers;
using schedule_automation_app_server.Domain.Common;
using schedule_automation_app_server.Domain.Entities;
using schedule_automation_app_server.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.Application.Services.Implementation;

namespace schedule_automation_app_server.Tests;

public class SubjectMapperTests
{
    private static ComponentDto MakeDto(
        string name = "Экзамен",
        double weight = 100,
        int complexity = 5,
        double currentGrade = 5,
        bool isGraded = false,
        bool isBlocking = false,
        double minimumGrade = 0,
        bool isAutoGrade = false,
        double autoGradeMinScore = 0) => new ComponentDto
    {
        Name = name,
        Weight = weight,
        Complexity = complexity,
        CurrentGrade = currentGrade,
        IsGraded = isGraded,
        IsBlocking = isBlocking,
        MinimumGrade = minimumGrade,
        IsAutoGrade = isAutoGrade,
        AutoGradeMinScore = autoGradeMinScore
    };

    [Fact]
    public void ToDomain_ValidRequest_CreatesSubjectWithCorrectName()
    {
        CreateSubjectRequest request = new CreateSubjectRequest
        {
            Name = "Математика",
            TargetGrade = 7,
            Components = new List<ComponentDto> { MakeDto() }
        };

        Subject subject = SubjectMapper.ToDomain(request);

        Assert.Equal("Математика", subject.Name);
        Assert.Equal(7, subject.TargetGrade);
    }

    [Fact]
    public void ToDomain_Request_MapsComponentsCorrectly()
    {
        CreateSubjectRequest request = new CreateSubjectRequest
        {
            Name = "Физика",
            TargetGrade = 6,
            Components = new List<ComponentDto>
            {
                MakeDto(name: "Экзамен", weight: 60, complexity: 8, currentGrade: 3, isGraded: false),
                MakeDto(name: "ДЗ", weight: 40, complexity: 3, currentGrade: 7, isGraded: true)
            }
        };

        Subject subject = SubjectMapper.ToDomain(request);

        Assert.Equal(2, subject.Components.Count);
        Assert.Equal("Экзамен", subject.Components[0].Name);
        Assert.Equal(60, subject.Components[0].Weight);
        Assert.Equal(8, subject.Components[0].Complexity);
        Assert.Equal(3, subject.Components[0].CurrentGrade);
        Assert.False(subject.Components[0].IsGraded);
        Assert.True(subject.Components[1].IsGraded);
    }

    [Fact]
    public void ToDomain_ComponentWithBlockingAndAutoGrade_MapsFlags()
    {
        CreateSubjectRequest request = new CreateSubjectRequest
        {
            Name = "Химия",
            TargetGrade = 7,
            Components = new List<ComponentDto>
            {
                MakeDto(isBlocking: true, minimumGrade: 4.0, isAutoGrade: true, autoGradeMinScore: 7.0)
            }
        };

        Subject subject = SubjectMapper.ToDomain(request);

        GradeComponent component = subject.Components[0];
        Assert.True(component.IsBlocking);
        Assert.Equal(4.0, component.MinimumGrade);
        Assert.True(component.IsAutoGrade);
        Assert.Equal(7.0, component.AutoGradeMinScore);
    }

    [Fact]
    public void ToListItemResponse_ReturnsCorrectFields()
    {
        Subject subject = TestHelpers.CreateSubject(
            name: "Алгебра", targetGrade: 8, components: ("Экзамен", 100, 5, 6, false, false, 0, false, 0));

        SubjectListItemResponse response = SubjectMapper.ToListItemResponse(subject, 6.0);

        Assert.Equal("Алгебра", response.Name);
        Assert.Equal(8, response.TargetGrade);
        Assert.Equal(6.0, response.CurrentGrade);
        Assert.Equal(1, response.ComponentCount);
    }

    [Fact]
    public void ToListItemResponse_RoundsCurrentGrade()
    {
        Subject subject = TestHelpers.CreateSubject(
            name: "Тест", targetGrade: 7, components: ("А", 100, 5, 5, false, false, 0, false, 0));

        SubjectListItemResponse response = SubjectMapper.ToListItemResponse(subject, 6.666666);

        Assert.Equal(6.67, response.CurrentGrade);
    }

    [Fact]
    public void ToResponse_WithNoAutoComponents_HasAutoGradeOptionFalse()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7, components: ("Экзамен", 100, 5, 5, false, false, 0, false, 0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

        Assert.False(response.HasAutoGradeOption);
        Assert.Null(response.PlanWithAuto);
    }

    [Fact]
    public void ToResponse_WithAutoComponent_HasAutoGradeOptionTrue()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7, components: ("Экзамен", 100, 5, 5, false, false, 0, true, 8.0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);
        OptimizationPlan planWithAuto = service.CalculateOptimizationPlanWithAuto(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan, null, planWithAuto);

        Assert.True(response.HasAutoGradeOption);
        Assert.NotNull(response.PlanWithAuto);
    }

    [Fact]
    public void ToResponse_MapsOptimizationItems()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 8,
            components: [
                ("Экзамен", 60, 7, 0, false, false, 0, false, 0),
                ("ДЗ", 40, 3, 5, false, false, 0, false, 0)
            ]);

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan);

        Assert.Equal("Экзамен", response.Components[0].Name);
        Assert.Equal(2, response.Components.Count);
    }

    [Fact]
    public void ToResponse_WithPlansRange_MapsRange()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7, components: ("Все", 100, 5, 0, false, false, 0, false, 0));

        GradeCalculationService service = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        OptimizationPlan plan = service.CalculateOptimizationPlan(subject);
        List<OptimizationPlan> range = service.CalculatePlansRange(subject);

        SubjectResponse response = SubjectMapper.ToResponse(subject, plan, range);

        Assert.NotNull(response.PlansRange);
        Assert.Equal(4, response.PlansRange.Count);
    }
}