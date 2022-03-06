using System.Text;
using System.Text.Json;
using FluffySpoon.AspNet.Ngrok.New.Models;
using Microsoft.Extensions.Logging;

namespace FluffySpoon.AspNet.Ngrok.New;

public class NgrokApiClient : INgrokApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<NgrokApiClient> _logger;

    public NgrokApiClient(
        HttpClient httpClient,
        ILogger<NgrokApiClient> logger)
    {
        _client = httpClient;
        _logger = logger;
    }

    public async Task<Tunnel> CreateTunnelAsync(
        string projectName, 
        string address,
        CancellationToken cancellationToken)
    {
        var url = new Uri(address);

        var request = new CreateTunnelApiRequest()
        {
            Name = projectName,
            Address = url.Host + ":" + url.Port,
            Protocol = "http",
            HostHeader = address
        };

        var json = JsonSerializer.Serialize(request);

        while (true)
        {
            _logger.LogInformation("Creating tunnel {TunnelName}", request.Name);
            
            var response = await _client.PostAsync(
                "/api/tunnels",
                new StringContent(json, Encoding.UTF8, "application/json"),
                cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Tunnel created");
                return JsonSerializer.Deserialize<Tunnel>(responseText)!;
            }

            var error = JsonSerializer.Deserialize<ErrorResponse>(responseText);
            _logger.LogInformation("Tunnel creation failed: {ErrorMessage}", error.Message);

            const int errorCodeNgrokNotReadyToStartTunnels = 104;
            if (error?.ErrorCode != errorCodeNgrokNotReadyToStartTunnels)
            {
                throw new InvalidOperationException(
                    $"Could not create tunnel for {projectName} ({address}): {responseText}");
            }

            await Task.Delay(100);
        }
    }
}