﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffySpoon.Ngrok;

public class NgrokProcess : INgrokProcess
{
    private readonly IOptionsMonitor<NgrokOptions> _options;
    private readonly ILogger<NgrokProcess> _logger;
    private readonly INgrokApiClient _ngrokApiClient;

    public NgrokProcess(
        IOptionsMonitor<NgrokOptions> options,
        ILogger<NgrokProcess> logger,
        INgrokApiClient ngrokApiClient)
    {
        _options = options;
        _logger = logger;
        _ngrokApiClient = ngrokApiClient;
    }

    public async Task StartAsync()
    {
        await KillExistingProcessesAsync();

        _logger.LogInformation("Starting Ngrok process");

        var processInformation = GetProcessStartInfo();
        using var process =
            Process.Start(processInformation) ??
            throw new InvalidOperationException("Could not start process");

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        while (!await _ngrokApiClient.IsNgrokReady(cancellationToken))
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    private ProcessWindowStyle GetProcessWindowStyle()
    {
        return _options.CurrentValue.ShowNgrokWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
    }

    private async Task KillExistingProcessesAsync()
    {
        var existingProcesses = Process
            .GetProcessesByName(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Ngrok" : "ngrok")
            .ToArray();
        if (!existingProcesses.Any())
            return;

        try
        {
            _logger.LogDebug("Killing existing ngrok processes");

            foreach (var existingProcess in existingProcesses)
            {
                existingProcess.Kill();
                await existingProcess.WaitForExitAsync();
            }
        }
        finally
        {
            foreach (var existingProcess in existingProcesses)
            {
                existingProcess.Dispose();
            }
        }
    }

    private ProcessStartInfo GetProcessStartInfo()
    {
        if (string.IsNullOrEmpty(_options.CurrentValue.AuthToken))
        {
            throw new InvalidOperationException("An auth token is required for Ngrok to work.");
        }

        var additionalArguments = $"--authtoken {_options.CurrentValue.AuthToken}";

        var processStartInfo = new ProcessStartInfo(
            NgrokDownloader.GetExecutableFileName(),
            $"start --none {additionalArguments}")
        {
            CreateNoWindow = true,
            WindowStyle = GetProcessWindowStyle(),
            UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardError = false,
            RedirectStandardOutput = false,
            RedirectStandardInput = false
        };
        return processStartInfo;
    }

    public async Task StopAsync()
    {
        await KillExistingProcessesAsync();
    }
}