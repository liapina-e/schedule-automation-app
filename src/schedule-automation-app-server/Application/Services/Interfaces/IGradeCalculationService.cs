using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Application.Services.Interfaces;

public interface IGradeCalculationService
{
    double CalculateCurrentGrade(Subject subject);
    OptimizationPlan CalculateOptimizationPlan(Subject subject);
    OptimizationPlan CalculateOptimizationPlanForGrade(Subject subject, int targetGrade);
    OptimizationPlan CalculateOptimizationPlanWithAuto(Subject subject);
    List<OptimizationPlan> CalculatePlansRange(Subject subject);
    WhatIfResponse CalculateWhatIf(Subject subject, List<WhatIfComponentDto> hypotheticalGrades);
}