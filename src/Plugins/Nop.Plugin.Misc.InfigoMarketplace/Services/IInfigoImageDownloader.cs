using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Fetches an image from a presigned URL returned by the Infigo API. A failed download yields <c>null</c> so the
/// import can carry on without the image — a broken image should never abort the whole package import.
/// </summary>
public interface IInfigoImageDownloader
{
    Task<DownloadedImage?> TryDownloadAsync(string url, CancellationToken ct = default);
}
