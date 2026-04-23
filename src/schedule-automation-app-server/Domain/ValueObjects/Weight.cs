using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.ValueObjects;

public class Weight : ValueObject
{
    public double Value { get; }

    public Weight(double value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentException("Вес должен быть от 0 до 100.");
        }

        Value = value;
    }

    private Weight() { }
    
    public double AsDecimal() => Value / 100.0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
