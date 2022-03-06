﻿// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace FluffySpoon.AspNet.Ngrok.New;

public class NgrokDownloader : INgrokDownloader
{
	private readonly HttpClient _httpClient;

	public NgrokDownloader(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task DownloadExecutableAsync(CancellationToken cancellationToken)
	{
		var downloadUrl = GetDownloadPath();
		var fileName = $"{GetOsArchitectureString()}.zip";
		var filePath = $"{Path.Combine(Directory.GetCurrentDirectory(), fileName)}";
		if (File.Exists(filePath))
			return;

		var downloadResponse = await _httpClient.GetAsync(downloadUrl, cancellationToken);
		downloadResponse.EnsureSuccessStatusCode();

		var downloadStream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
		await using var writer = File.Create(filePath);
		await downloadStream.CopyToAsync(writer, cancellationToken);

		ZipFile.ExtractToDirectory(filePath, Directory.GetCurrentDirectory());

		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			await GrantNgrokFileExecutablePermissions();
	}

	private static async Task GrantNgrokFileExecutablePermissions()
	{
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "/bin/bash",
				Arguments = $"-c \"chmod +x {Directory.GetCurrentDirectory()}/ngrok\""
			}
		};

		process.Start();
		await process.WaitForExitAsync();
	}
        
	private static string GetDownloadPath()
	{
		const string cdnPath = "c/4VmDzA7iaHb/Ngrok-stable";
		return $"{cdnPath}-{GetOsArchitectureString()}.zip";
	}
        
	private static string GetArchitectureString()
	{
		var architecture = RuntimeInformation.ProcessArchitecture;
		switch (architecture)
		{
			case Architecture.Arm:
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					throw new NgrokUnsupportedException();
				}
				return "arm";
			case Architecture.Arm64:
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					throw new NgrokUnsupportedException();
				}
				return "arm64";
			
			case Architecture.X64:
				return "amd64";
			
			case Architecture.X86:
				return "386";
			
			case Architecture.Wasm:
			case Architecture.S390x:
			default:
				throw new NgrokUnsupportedException();
		}
	}

	private static string GetOsString()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return "windows";
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return "linux";
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return "darwin";
		}
		throw new NgrokUnsupportedException();
	}

	private static string GetOsArchitectureString()
	{
		return $"{GetOsString()}-{GetArchitectureString()}";
	}
}