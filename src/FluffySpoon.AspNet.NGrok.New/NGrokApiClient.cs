// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 David Prothero, Kevin Gysberg

using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using FluffySpoon.AspNet.Ngrok.New.Models;
using Microsoft.Extensions.Logging;

namespace FluffySpoon.AspNet.Ngrok.New;

public class NgrokApiClient
{
    private readonly HttpClient _nGrokApi;
    private readonly ILogger _logger;
    private readonly NgrokProcess _nGrokProcess;
    private readonly NgrokOptions _options;

    public NgrokApiClient(HttpClient httpClient, NgrokProcess nGrokProcess, NgrokOptions options, ILogger<NgrokApiClient> logger)
    {
        _nGrokApi = httpClient;
        _options = options;
        _nGrokProcess = nGrokProcess;
        _logger = logger;
    }

    internal async Task<IEnumerable<Tunnel>?> StartTunnelsAsync(string url, CancellationToken cancellationToken)
    {
        await StartNgrokAsync(cancellationToken);

        if (await HasTunnelByAddressAsync(url, cancellationToken))
            return await GetTunnelListAsync(cancellationToken);

        var tunnel = await CreateTunnelAsync(System.AppDomain.CurrentDomain.FriendlyName, url, cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            await Task.Delay(100, cancellationToken);

            if (await HasTunnelByAddressAsync(tunnel?.Config?.Addr, cancellationToken))
                return await GetTunnelListAsync(cancellationToken);
        }

        throw new Exception("A timeout occured while waiting for the created tunnel to exist.");
    }

    private async Task<bool> HasTunnelByAddressAsync(string address, CancellationToken cancellationToken)
    {
        var tunnels = await GetTunnelListAsync(cancellationToken);
        return tunnels != null && tunnels.Any(x => x.Config?.Addr == address);
    }

    /// <returns></returns>
    private async Task StartNgrokAsync(CancellationToken cancellationToken)
    {
        _nGrokProcess.StartNgrokProcess();

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var canGetTunnelList = false;
            while (!canGetTunnelList && stopwatch.Elapsed < TimeSpan.FromSeconds(30))
            {
                canGetTunnelList = await CanGetTunnelList(cancellationToken);
                await Task.Delay(100, cancellationToken);
            }

            if (!canGetTunnelList)
            {
                throw new Exception("A timeout occured while waiting for the Ngrok process.");
            }
        }
        catch (Exception ex)
        {
            throw new NgrokStartFailedException(ex);
        }
    }

    internal void StopNgrok()
    {
        _nGrokProcess.Stop();
    }

    private async Task<bool> CanGetTunnelList(CancellationToken cancellationToken)
    {
        var tunnels = await GetTunnelListAsync(cancellationToken);
        return tunnels != null;
    }

    private async Task<Tunnel[]?> GetTunnelListAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _nGrokApi.GetAsync("/api/tunnels", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseText = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<NgrokTunnelsApiResponse>(responseText);
            return apiResponse.Tunnels;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<Tunnel> CreateTunnelAsync(string projectName, string address, CancellationToken cancellationToken)
    {
        var url = new Uri(address);

        var request = new NgrokTunnelApiRequest
        {
            Name = projectName,
            Addr = url.Host + ":" + url.Port,
            Proto = "http",
            HostHeader = address
        };

        var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        Debug.WriteLine($"request: '{json}'");

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        while (true)
        {
            var response = await _nGrokApi.PostAsync("/api/tunnels", httpContent, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Tunnel>(responseText);

            var error = JsonConvert.DeserializeObject<NgrokErrorApiResult>(responseText);

            var ERROR_CODE_NGROK_NOT_READY_TO_START_TUNNELS = 104;
            if (error.ErrorCode == ERROR_CODE_NGROK_NOT_READY_TO_START_TUNNELS)
            {
                await Task.Delay(100, cancellationToken);
                continue;
            }

            throw new InvalidOperationException(
                $"Could not create tunnel for {projectName} ({address}): " + responseText);
        }
    }
}