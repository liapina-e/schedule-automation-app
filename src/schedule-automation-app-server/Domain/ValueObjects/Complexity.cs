using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Domain.ValueObjects;

public class Complexity : ValueObject
{
    public int Value { get; }

    public Complexity(int value)
    {
        if (value < 1 || value > 10)
        {
            throw new ArgumentException("Сложность должна быть от 1 до 10.");
        }

        Value = value;
    }

    private Complexity() { }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
