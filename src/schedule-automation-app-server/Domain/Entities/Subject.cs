using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int TargetGrade { get; private set; }
    public List<GradeComponent> Components { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    private Subject() { }

    public Subject(string name, int targetGrade)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название предмета не может быть пустым.");
        }

        if (targetGrade < 4 || targetGrade > 10)
        {
            throw new ArgumentException("Целевая оценка должна быть от 4 до 10.");
        }

        Name = name;
        TargetGrade = targetGrade;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddComponent(GradeComponent component)
    {
        if (component == null)
        {
            throw new ArgumentNullException(nameof(component));
        }

        Components.Add(component);
    }
}
