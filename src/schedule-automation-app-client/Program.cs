using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Velopack;

namespace schedule_automation_app_client;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        StartServerIfNotRunning();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void StartServerIfNotRunning()
    {
        try
        {
            string appDir = AppContext.BaseDirectory;
            string serverExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "schedule-automation-app-server.exe"
                : "schedule-automation-app-server";

            string serverPath = Path.Combine(appDir, serverExeName);

            if (!File.Exists(serverPath))
            {
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось запустить сервер: {ex.Message}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}