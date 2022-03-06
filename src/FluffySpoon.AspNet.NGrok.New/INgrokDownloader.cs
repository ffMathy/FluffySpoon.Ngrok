namespace FluffySpoon.AspNet.Ngrok.New;

public interface INgrokDownloader
{
    Task DownloadExecutableAsync(CancellationToken cancellationToken);
}