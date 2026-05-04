using System;
using System.Collections.Generic;

namespace schedule_automation_app_client.Services.Dtos;

public record CreateSubjectRequestDto(
    string Name,
    int TargetGrade,
    List<ComponentRequestDto> Components);

public record SubjectListItemDto(
    Guid Id,
    string Name,
    int TargetGrade,
    double CurrentGrade,
    int ComponentCount);