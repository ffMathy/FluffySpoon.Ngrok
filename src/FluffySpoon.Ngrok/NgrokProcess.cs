using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffySpoon.Ngrok;

public class NgrokProcess : INgrokProcess
{
    private readonly IOptionsMonitor<NgrokOptions> _options;
    private readonly ILogger<NgrokProcess> _logger;

    private Process? _process;

    public NgrokProcess(
        IOptionsMonitor<NgrokOptions> options,
        ILogger<NgrokProcess> logger)
    {
        _options = options;
        _logger = logger;
    }

    public void Start()
    {
        var processInformation = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            GetWindowsProcessStartInfo() :
            GetLinuxProcessStartInfo();
        
        _logger.LogInformation("Starting ngrok process");
        _process = Process.Start(processInformation);
    }

    private ProcessWindowStyle GetProcessWindowStyle()
    {
        return _options.CurrentValue.ShowNgrokWindow ? 
            ProcessWindowStyle.Normal : 
            ProcessWindowStyle.Hidden;
    }

    private ProcessStartInfo GetWindowsProcessStartInfo()
    {
        var windowsProcessStartInfo = new ProcessStartInfo("Ngrok.exe", "start --none")
        {
            CreateNoWindow = true,
            WindowStyle = GetProcessWindowStyle(),
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory
        };
        return windowsProcessStartInfo;
    }

    private ProcessStartInfo GetLinuxProcessStartInfo()
    {
        var linuxProcessStartInfo =
            new ProcessStartInfo("/bin/bash", "-c \"" + Directory.GetCurrentDirectory() + "/ngrok start --none\"")
            {
                CreateNoWindow = true,
                WindowStyle = GetProcessWindowStyle(),
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory
            };
        return linuxProcessStartInfo;
    }

    public void Stop()
    {
        _logger.LogInformation("Stopping ngrok process");
        
        _process?.Kill();
        _process = null;
    }

    public static void KillAll()
    {
        foreach (var p in Process.GetProcessesByName("ngrok"))
        {
            p.Kill();
        }
    }
}