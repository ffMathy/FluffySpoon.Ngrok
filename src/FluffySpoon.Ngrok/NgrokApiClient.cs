using FluffySpoon.Ngrok.Models;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public class NgrokApiClient : INgrokApiClient
{
    private readonly IFlurlClient _client;
    private readonly ILogger<NgrokApiClient> _logger;
    private readonly NgrokOptions _ngrokOptions;

    public NgrokApiClient(
        HttpClient httpClient,
        ILogger<NgrokApiClient> logger,
        NgrokOptions ngrokOptions)
    {
        _client = new FlurlClient(httpClient)
        {
            Settings = new ClientFlurlHttpSettings()
            {
                JsonSerializer = new NewtonsoftJsonSerializer(
                    new JsonSerializerSettings()),
                Timeout = TimeSpan.FromSeconds(10)
            }
        };
        _logger = logger;
        _ngrokOptions = ngrokOptions;
    }

    public async Task<TunnelResponse[]> GetTunnelsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tunnels = await CreateRequest("tunnels")
                .GetJsonAsync<TunnelListResponse>(cancellationToken);
            return tunnels.Tunnels.ToArray();
        }
        catch (FlurlHttpException ex)
        {
            _logger.LogError(ex, "Could not list tunnels");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occured during tunnel fetching");
            throw;
        }
    }

    public async Task<TunnelResponse> CreateTunnelAsync(
        string projectName, 
        Uri address,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Creating tunnel {TunnelName}", projectName);

            try
            {
                var response = await CreateRequest("tunnels")
                    .PostJsonAsync(
                        new CreateTunnelApiRequest()
                        {
                            Name = projectName,
                            Address = address.Host + ":" + address.Port,
                            Protocol = address.Scheme,
                            HostHeader = address
                                .ToString()
                                .TrimEnd('/')
                        },
                        cancellationToken)
                    .ReceiveJson<TunnelResponse>();
                _logger.LogInformation("Tunnel {@Tunnel} created", response);

                return response;
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.Call.Response.GetJsonAsync<ErrorResponse>();

                var isNotReadyToStartTunnels = error.ErrorCode == 104;
                if (!isNotReadyToStartTunnels)
                {
                    _logger.LogError(ex, "Tunnel creation failed: {@Error}", error);
                    throw;
                }

                _logger.LogDebug(ex, "Tunnel creation failed due to Ngrok not being ready: {@Error}", error);
                await Task.Delay(25, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occured during tunnel creation");
                throw;
            }
        }
        
        throw new OperationCanceledException();
    }

    private IFlurlRequest CreateRequest(params object[] pathSegments)
    {
        var request = _client.Request(pathSegments);
        if (_ngrokOptions.AuthToken != null)
            request = request.WithOAuthBearerToken(_ngrokOptions.AuthToken);
        
        return request;
    }
}