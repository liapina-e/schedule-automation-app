namespace schedule_automation_app_client.Services.Dtos;

public record AutoGradeComponentInfoDto(
    string ComponentName,
    double CurrentGrade,
    double RequiredMinScore,
    bool IsAlreadyAchieved);