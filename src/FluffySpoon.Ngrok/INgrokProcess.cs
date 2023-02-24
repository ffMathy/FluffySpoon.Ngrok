namespace FluffySpoon.Ngrok;

public interface INgrokProcess
{
    Task StartAsync();
    Task StopAsync();
}