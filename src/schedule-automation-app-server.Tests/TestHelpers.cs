using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public static class TestHelpers
{
    public static Subject CreateSubject(
        string name = "Тест",
        int targetGrade = 7,
        params (string name, double weight, int complexity, double currentGrade, bool isGraded, bool isBlocking, double minimumGrade, bool isAutoGrade, double autoGradeMinScore)[] components)
    {
        Subject subject = new Subject(name, targetGrade);

        foreach (var c in components)
        {
            GradeComponent component = new GradeComponent(
                c.name, c.weight, c.complexity, c.currentGrade, c.isGraded)
            {
                IsBlocking = c.isBlocking,
                MinimumGrade = c.minimumGrade,
                IsAutoGrade = c.isAutoGrade,
                AutoGradeMinScore = c.autoGradeMinScore
            };

            subject.AddComponent(component);
        }

        return subject;
    }
}