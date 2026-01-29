using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int TargetGrade { get; private set; }
    public List<GradeComponent> Components { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    public Subject(string name, int targetGrade)
    {
        Name = name;
        TargetGrade = targetGrade;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void AddComponent(GradeComponent component)
    {
        Components.Add(component);
    }
    
    public double CalculateCurrentGrade()
    {
        if (Components.Count == 0)
        {
            return 0;
        }
        
        double totalWeight = Components.Sum(c => c.Weight.Value);
        double weightedSum = Components.Sum(c => c.CurrentGrade.Value * c.Weight.Value);

        return weightedSum / totalWeight;
    }
}