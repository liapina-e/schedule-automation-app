using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.ValueObjects;

public class Grade : ValueObject
{
    public double Value { get; }
    
    public Grade(double value)
    {
        if (value < 0 || value > 10)
        {
            throw new ArgumentException("Оценка должна быть от в диапазоне от 0 до 10. ");
        }
        Value = value;
    }
    
    private Grade() { }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}