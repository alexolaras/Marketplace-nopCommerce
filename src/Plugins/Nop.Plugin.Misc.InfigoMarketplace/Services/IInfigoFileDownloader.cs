using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Fetches a package binary from a presigned URL returned by the Infigo API. A failed download yields
/// <c>null</c> so the import can carry on without the file — a broken download should not abort the whole import.
/// </summary>
public interface IInfigoFileDownloader
{
    Task<DownloadedFile?> TryDownloadAsync(string url, CancellationToken ct = default);
}
