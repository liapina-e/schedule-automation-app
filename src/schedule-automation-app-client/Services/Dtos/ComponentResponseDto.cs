namespace schedule_automation_app_client.Services.Dtos;

public record ComponentResponseDto(
    string Name,
    double Weight,
    int Complexity,
    double CurrentGrade,
    bool IsBlocking,
    double MinimumGrade,
    bool IsGraded,
    bool IsAutoGrade,
    double AutoGradeMinScore);