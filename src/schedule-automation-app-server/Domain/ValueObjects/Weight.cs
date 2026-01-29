using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.ValueObjects;

public class Weight : ValueObject
{
    public double Value { get; }
    
    public Weight(double value)
    {
        if (value < 0 || value > 1)
        {
            throw new ArgumentException("Вес должен быть в диапазоне от 0 до 1. ");
        }
        Value = value;
    }
    
    private Weight() { }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}