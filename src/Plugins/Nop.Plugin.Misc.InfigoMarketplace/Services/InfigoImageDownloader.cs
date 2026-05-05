using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoImageDownloader(
    HttpClient httpClient,
    IPictureService pictureService,
    ILogger logger) : IInfigoImageDownloader
{
    public async Task<DownloadedImage?> TryDownloadAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            await logger.WarningAsync($"Infigo image: skipping malformed URL '{url}'.");
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                await logger.WarningAsync(
                    $"Infigo image: download from {uri.GetLeftPart(UriPartial.Path)} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            if (bytes.Length == 0)
                return null;

            var mimeType = response.Content.Headers.ContentType?.MediaType
                ?? pictureService.GetPictureContentTypeByFileExtension(Path.GetExtension(uri.AbsolutePath));

            if (string.IsNullOrWhiteSpace(mimeType))
            {
                await logger.WarningAsync(
                    $"Infigo image: could not determine content type for {uri.GetLeftPart(UriPartial.Path)}.");
                return null;
            }

            return new DownloadedImage(bytes, mimeType);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            await logger.WarningAsync($"Infigo image: download from {uri.GetLeftPart(UriPartial.Path)} timed out.");
            return null;
        }
        catch (HttpRequestException ex)
        {
            await logger.WarningAsync(
                $"Infigo image: download from {uri.GetLeftPart(UriPartial.Path)} failed — {ex.Message}");
            return null;
        }
    }
}
