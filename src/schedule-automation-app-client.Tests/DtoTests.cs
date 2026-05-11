using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Services.Dtos;
using schedule_automation_app_client.Services;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class DtoTests
{
    [AvaloniaFact]
    public void ComponentRequestDto_Constructor_StoresAllFields()
    {
        ComponentRequestDto dto = new ComponentRequestDto(
            Name: "Экзамен",
            Weight: 50,
            Complexity: 8,
            CurrentGrade: 6,
            IsBlocking: true,
            MinimumGrade: 4.0,
            IsGraded: false,
            IsAutoGrade: true,
            AutoGradeMinScore: 8.0);

        Assert.Equal("Экзамен", dto.Name);
        Assert.Equal(50, dto.Weight);
        Assert.Equal(8, dto.Complexity);
        Assert.Equal(6, dto.CurrentGrade);
        Assert.True(dto.IsBlocking);
        Assert.Equal(4.0, dto.MinimumGrade);
        Assert.False(dto.IsGraded);
        Assert.True(dto.IsAutoGrade);
        Assert.Equal(8.0, dto.AutoGradeMinScore);
    }

    [AvaloniaFact]
    public void ComponentRequestDto_EqualValues_AreEqual()
    {
        ComponentRequestDto a = new ComponentRequestDto("А", 50, 5, 5, false, 0, false, false, 0);
        ComponentRequestDto b = new ComponentRequestDto("А", 50, 5, 5, false, 0, false, false, 0);
        Assert.Equal(a, b);
    }

    [AvaloniaFact]
    public void ComponentRequestDto_DifferentValues_AreNotEqual()
    {
        ComponentRequestDto a = new ComponentRequestDto("А", 50, 5, 5, false, 0, false, false, 0);
        ComponentRequestDto b = new ComponentRequestDto("Б", 50, 5, 5, false, 0, false, false, 0);
        Assert.NotEqual(a, b);
    }

    [AvaloniaFact]
    public void ComponentRequestDto_ToString_ContainsName()
    {
        ComponentRequestDto dto = new ComponentRequestDto("Экзамен", 50, 5, 5, false, 0, false, false, 0);
        Assert.Contains("Экзамен", dto.ToString());
    }
    
    [AvaloniaFact]
    public void ComponentResponseDto_Constructor_StoresAllFields()
    {
        ComponentResponseDto dto = new ComponentResponseDto(
            Name: "ДЗ",
            Weight: 30,
            Complexity: 3,
            CurrentGrade: 7,
            IsBlocking: false,
            MinimumGrade: 0,
            IsGraded: false,
            IsAutoGrade: false,
            AutoGradeMinScore: 0);

        Assert.Equal("ДЗ", dto.Name);
        Assert.Equal(30, dto.Weight);
        Assert.Equal(3, dto.Complexity);
        Assert.Equal(7, dto.CurrentGrade);
        Assert.False(dto.IsBlocking);
    }

    [AvaloniaFact]
    public void ComponentResponseDto_EqualValues_AreEqual()
    {
        ComponentResponseDto a = new ComponentResponseDto("А", 50, 5, 5, false, 0, false, false, 0);
        ComponentResponseDto b = new ComponentResponseDto("А", 50, 5, 5, false, 0, false, false, 0);
        Assert.Equal(a, b);
    }
    
    [AvaloniaFact]
    public void CreateSubjectRequestDto_Constructor_StoresFields()
    {
        List<ComponentRequestDto> components = new List<ComponentRequestDto>
        {
            new ComponentRequestDto("А", 100, 5, 5, false, 0, false, false, 0)
        };

        CreateSubjectRequestDto dto = new CreateSubjectRequestDto("Математика", 7, components);

        Assert.Equal("Математика", dto.Name);
        Assert.Equal(7, dto.TargetGrade);
        Assert.Single(dto.Components);
    }
    
    [AvaloniaFact]
    public void SubjectListItemDto_Constructor_StoresFields()
    {
        Guid id = Guid.NewGuid();
        SubjectListItemDto dto = new SubjectListItemDto(id, "Физика", 8, 6.5, 3);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Физика", dto.Name);
        Assert.Equal(8, dto.TargetGrade);
        Assert.Equal(6.5, dto.CurrentGrade);
        Assert.Equal(3, dto.ComponentCount);
    }
    
    [AvaloniaFact]
    public void OptimizationItemDto_Constructor_StoresFields()
    {
        Guid componentId = Guid.NewGuid();
        OptimizationItemDto dto = new OptimizationItemDto(
            ComponentId: componentId,
            ComponentName: "Экзамен",
            CurrentGrade: 3,
            RequiredGrade: 7.5,
            Priority: 1,
            Reason: "Низкая стоимость");

        Assert.Equal(componentId, dto.ComponentId);
        Assert.Equal("Экзамен", dto.ComponentName);
        Assert.Equal(3, dto.CurrentGrade);
        Assert.Equal(7.5, dto.RequiredGrade);
        Assert.Equal(1, dto.Priority);
        Assert.Equal("Низкая стоимость", dto.Reason);
    }
    
    [AvaloniaFact]
    public void GradePlanDto_Constructor_StoresFields()
    {
        GradePlanDto dto = new GradePlanDto(
            TargetGrade: 8,
            IsAchievable: true,
            Recommendation: "Рек",
            Plan: new List<OptimizationItemDto>());

        Assert.Equal(8, dto.TargetGrade);
        Assert.True(dto.IsAchievable);
        Assert.Equal("Рек", dto.Recommendation);
        Assert.NotNull(dto.Plan);
    }
    
    [AvaloniaFact]
    public void AutoGradeComponentInfoDto_Constructor_StoresFields()
    {
        AutoGradeComponentInfoDto dto = new AutoGradeComponentInfoDto(
            ComponentName: "ДЗ",
            CurrentGrade: 6,
            RequiredMinScore: 8,
            IsAlreadyAchieved: false);

        Assert.Equal("ДЗ", dto.ComponentName);
        Assert.Equal(6, dto.CurrentGrade);
        Assert.Equal(8, dto.RequiredMinScore);
        Assert.False(dto.IsAlreadyAchieved);
    }
    
    [AvaloniaFact]
    public void AutoGradePlanDto_Constructor_StoresFields()
    {
        AutoGradePlanDto dto = new AutoGradePlanDto(
            IsAchievable: true,
            Recommendation: "Подтянуть ДЗ",
            Plan: new List<OptimizationItemDto>(),
            AutoGradeComponents: new List<AutoGradeComponentInfoDto>());

        Assert.True(dto.IsAchievable);
        Assert.Equal("Подтянуть ДЗ", dto.Recommendation);
        Assert.NotNull(dto.Plan);
        Assert.NotNull(dto.AutoGradeComponents);
    }
    
    [AvaloniaFact]
    public void PlanResponseDto_Constructor_StoresAllFields()
    {
        Guid id = Guid.NewGuid();
        PlanResponseDto dto = new PlanResponseDto(
            Id: id,
            Name: "Математика",
            CurrentGrade: 4.5,
            TargetGrade: 7,
            IsAchievable: true,
            Recommendation: "План",
            OptimalPlan: new List<OptimizationItemDto>(),
            PlansRange: new List<GradePlanDto>(),
            HasAutoGradeOption: false,
            PlanWithAuto: null,
            Components: null);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Математика", dto.Name);
        Assert.Equal(4.5, dto.CurrentGrade);
        Assert.Equal(7, dto.TargetGrade);
        Assert.True(dto.IsAchievable);
        Assert.False(dto.HasAutoGradeOption);
        Assert.Null(dto.PlanWithAuto);
        Assert.Null(dto.Components);
    }

    [AvaloniaFact]
    public void PlanResponseDto_WithAutoGradePlan_StoresIt()
    {
        AutoGradePlanDto autoPlan = new AutoGradePlanDto(
            true, "Авто",
            new List<OptimizationItemDto>(),
            new List<AutoGradeComponentInfoDto>());

        PlanResponseDto dto = new PlanResponseDto(
            Guid.NewGuid(), "Тест", 5, 7, true, "План",
            new List<OptimizationItemDto>(), new List<GradePlanDto>(),
            true, autoPlan, null);

        Assert.True(dto.HasAutoGradeOption);
        Assert.NotNull(dto.PlanWithAuto);
        Assert.True(dto.PlanWithAuto!.IsAchievable);
    }
    
    [AvaloniaFact]
    public void ApiError_AllValues_AreDistinct()
    {
        ApiError none = ApiError.None;
        ApiError unavail = ApiError.ServerUnavailable;
        ApiError validation = ApiError.ValidationFailed;

        Assert.NotEqual(none, unavail);
        Assert.NotEqual(none, validation);
        Assert.NotEqual(unavail, validation);
    }
    
    [AvaloniaFact]
    public void AppSettings_ServerBaseUrl_IsLocalhost5284()
    {
        Assert.Equal("http://localhost:5284", AppSettings.ServerBaseUrl);
    }
}