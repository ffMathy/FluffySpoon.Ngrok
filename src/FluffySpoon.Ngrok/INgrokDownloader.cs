namespace FluffySpoon.Ngrok;

public interface INgrokDownloader
{
    Task DownloadExecutableAsync(CancellationToken cancellationToken);
}