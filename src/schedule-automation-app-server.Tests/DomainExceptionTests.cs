using schedule_automation_app_server.Domain.Exceptions;

namespace schedule_automation_app_server.Tests;

public class DomainExceptionTests
{
    [Fact]
    public void DomainValidationException_StoresMessage()
    {
        DomainValidationException ex = new DomainValidationException("Тестовая ошибка");
        Assert.Equal("Тестовая ошибка", ex.Message);
    }

    [Fact]
    public void DomainValidationException_IsException()
    {
        DomainValidationException ex = new DomainValidationException("Ошибка");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}