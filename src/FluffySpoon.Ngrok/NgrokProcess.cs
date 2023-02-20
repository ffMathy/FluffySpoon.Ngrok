using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace FluffySpoon.Ngrok;

public class NgrokProcess : INgrokProcess
{
    private readonly NgrokOptions _options;
    private readonly ILogger<NgrokProcess> _logger;

    private Process? _process;

    public NgrokProcess(
        NgrokOptions options,
        ILogger<NgrokProcess> logger)
    {
        _options = options;
        _logger = logger;
    }

    public void Start()
    {
        var processInformation = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? GetWindowsProcessStartInfo()
            : GetLinuxProcessStartInfo();

        var existingProcess = Process.GetProcessesByName(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            "Ngrok" :
            "ngrok");
        if (existingProcess.Any())
        {
            _logger.LogDebug("Ngrok process ({ProcessName}) is already running", processInformation.FileName);
            SetProcess(existingProcess.First());
            return;
        }

        _logger.LogInformation("Starting Ngrok process");

        var process = Process.Start(processInformation);
        SetProcess(process);
    }

    private void SetProcess(Process? process)
    {
        if (process == null)
            return;

        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += ProcessErrorDataReceived;

        _process = process;
    }

    private void ProcessErrorDataReceived(object? sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data))
            return;
        
        _logger.LogError("{Error}", e.Data);
    }

    private ProcessWindowStyle GetProcessWindowStyle()
    {
        return _options.ShowNgrokWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
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

        if (_process == null) 
            return;
        
        _process.ErrorDataReceived -= ProcessErrorDataReceived;
        _process.Kill();
        
        _process = null;
    }
}