using System.Collections.Generic;

namespace schedule_automation_app_client.Services.Dtos;

public record SubjectRequestDto(
    string Name,
    int TargetGrade,
    List<ComponentRequestDto> Components);