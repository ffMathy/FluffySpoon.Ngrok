// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2016 David Prothero
// Pulled from Github on 2019-01-13 at https://github.com/dprothero/NgrokExtensions

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;

namespace FluffySpoon.AspNet.Ngrok.Services
{
	public class NgrokProcess
	{
        private readonly NgrokOptions _ngrokOptions;
        private Process _process;

		public NgrokProcess(
            IHostApplicationLifetime applicationLifetime,
            NgrokOptions ngrokOptions)
        {
            _ngrokOptions = ngrokOptions;
            applicationLifetime.ApplicationStopping.Register(Stop);
        }

		public void StartNgrokProcess()
		{
            var processWindowStyle = _ngrokOptions.ShowNgrokWindow ? 
                ProcessWindowStyle.Normal : 
                ProcessWindowStyle.Hidden;

            var linuxProcessStartInfo = new ProcessStartInfo("/bin/bash", "-c \"" + Directory.GetCurrentDirectory() + "/ngrok start --none\"")
            {
                CreateNoWindow = true,
                WindowStyle = processWindowStyle,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory
            };

            var windowsProcessStartInfo = new ProcessStartInfo("Ngrok.exe", "start --none")
            {
                CreateNoWindow = true,
                WindowStyle = processWindowStyle,
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            var processInformation = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                windowsProcessStartInfo :
                linuxProcessStartInfo;

            Start(processInformation);
		}

		protected virtual void Start(ProcessStartInfo pi)
        {
            KillExistingNgrokProcesses();
            _process = Process.Start(pi);
        }

		public void Stop()
		{
            if (_process == null || _process.HasExited) 
                return;

            _process.Kill();
            KillExistingNgrokProcesses();
        }

        private static void KillExistingNgrokProcesses()
        {
            foreach (var p in Process.GetProcessesByName("ngrok"))
            {
                p.Kill();
            }
        }
    }
}
