namespace schedule_automation_app_client.Services.Dtos;

public record OptimizationItemDto(
    string ComponentName,
    double CurrentGrade,
    double RequiredGrade,
    int Priority,
    string Reason);