using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoFileDownloader(HttpClient httpClient, ILogger logger) : IInfigoFileDownloader
{
    public async Task<DownloadedFile?> TryDownloadAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            await logger.WarningAsync($"Infigo file: skipping malformed URL '{url}'.");
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                await logger.WarningAsync(
                    $"Infigo file: download from {uri.GetLeftPart(UriPartial.Path)} returned {(int)response.StatusCode} {response.ReasonPhrase}.");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            if (bytes.Length == 0)
                return null;

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            var disposition = response.Content.Headers.ContentDisposition;
            var fileName = disposition?.FileNameStar ?? disposition?.FileName;
            fileName = string.IsNullOrWhiteSpace(fileName)
                ? Path.GetFileName(uri.AbsolutePath)
                : fileName.Trim('"');
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "package";

            var extension = Path.GetExtension(fileName) ?? string.Empty;

            return new DownloadedFile(bytes, fileName, extension, contentType);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            await logger.WarningAsync($"Infigo file: download from {uri.GetLeftPart(UriPartial.Path)} timed out.");
            return null;
        }
        catch (HttpRequestException ex)
        {
            await logger.WarningAsync(
                $"Infigo file: download from {uri.GetLeftPart(UriPartial.Path)} failed — {ex.Message}");
            return null;
        }
    }
}
