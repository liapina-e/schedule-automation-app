namespace schedule_automation_app_client.Services.Dtos;

public record ComponentRequestDto(
    string Name,
    double Weight,
    int Complexity,
    double CurrentGrade,
    bool IsBlocking,
    double MinimumGrade,
    bool IsGraded);