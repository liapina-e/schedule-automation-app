using System.Reflection;
using Avalonia.Headless.XUnit;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class ProgramTests
{
    [AvaloniaFact]
    public void StartServerIfNotRunning_WhenServerExecutableMissing_DoesNotThrow()
    {
        System.Type? programType = typeof(App).Assembly.GetType("schedule_automation_app_client.Program");
        Assert.NotNull(programType);

        MethodInfo? method = programType!.GetMethod(
            "StartServerIfNotRunning",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);

        method!.Invoke(null, null);
    }

    [AvaloniaFact]
    public void BuildAvaloniaApp_ReturnsNonNull()
    {
        System.Type? programType = typeof(App).Assembly.GetType("schedule_automation_app_client.Program");
        Assert.NotNull(programType);

        MethodInfo? method = programType!.GetMethod(
            "BuildAvaloniaApp",
            BindingFlags.Static | BindingFlags.Public);

        Assert.NotNull(method);

        object? result = method!.Invoke(null, null);
        Assert.NotNull(result);
    }
}