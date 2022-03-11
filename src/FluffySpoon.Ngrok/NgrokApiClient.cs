using System.Net.Http.Headers;
using System.Text;
using FluffySpoon.Ngrok.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NgrokApi;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FluffySpoon.Ngrok;

public class NgrokApiClient : INgrokApiClient
{
    private readonly HttpClient _client;
    private readonly NgrokApi.Ngrok _ngrok;
    private readonly ILogger<NgrokApiClient> _logger;
    private readonly NgrokOptions _ngrokOptions;

    public NgrokApiClient(
        HttpClient httpClient,
        NgrokApi.Ngrok ngrok,
        ILogger<NgrokApiClient> logger,
        NgrokOptions ngrokOptions)
    {
        _client = httpClient;
        _ngrok = ngrok;
        _logger = logger;
        _ngrokOptions = ngrokOptions;
    }

    public async Task<Tunnel[]> GetTunnelsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _ngrok.Tunnels
                .List()
                .ToArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<Tunnel> CreateTunnelAsync(
        string projectName, 
        Uri address,
        CancellationToken cancellationToken)
    {
        var request = new CreateTunnelApiRequest()
        {
            Name = projectName,
            Address = address.Host + ":" + address.Port,
            Protocol = address.Scheme,
            HostHeader = address.ToString()
        };

        var json = JsonSerializer.Serialize(request);

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Creating tunnel {TunnelName}", request.Name);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if(_ngrokOptions.AuthToken != null)
            {
                content.Headers.Add("Authorization", $"Bearer {_ngrokOptions.AuthToken}");
            }
            
            var response = await _client.PostAsync(
                "/api/tunnels",
                content,
                cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Tunnel created");
                return JsonConvert.DeserializeObject<Tunnel>(responseText)!;
            }

            var error = JsonSerializer.Deserialize<ErrorResponse>(responseText);
            _logger.LogInformation("Tunnel creation failed: {ErrorMessage}", error?.Message);

            const int errorCodeNgrokNotReadyToStartTunnels = 104;
            if (error?.ErrorCode != errorCodeNgrokNotReadyToStartTunnels)
            {
                throw new InvalidOperationException(
                    $"Could not create tunnel for {projectName} ({address}): {responseText}");
            }

            await Task.Delay(25, cancellationToken);
        }
        
        throw new OperationCanceledException();
    }
}