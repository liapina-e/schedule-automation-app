using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace schedule_automation_app_client;

class Program
{
    private static Process? _serverProcess;

    [STAThread]
    public static void Main(string[] args)
    {
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

            bool alreadyRunning = System.Net.NetworkInformation.IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(ep => ep.Port == 5284);

            if (alreadyRunning)
            {
                return;
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xattr",
                    Arguments = $"-d com.apple.quarantine \"{serverPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();

                Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{serverPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            _serverProcess = Process.Start(startInfo);

            int attempts = 0;
            while (attempts < 30)
            {
                System.Threading.Thread.Sleep(500);
                bool up = System.Net.NetworkInformation.IPGlobalProperties
                    .GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(ep => ep.Port == 5284);
                if (up) break;
                attempts++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось запустить сервер: {ex.Message}");
        }
    }

    public static void StopServer()
    {
        try
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.WaitForExit(3000);
                _serverProcess.Dispose();
                _serverProcess = null;
            }
        }
        catch
        {
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}