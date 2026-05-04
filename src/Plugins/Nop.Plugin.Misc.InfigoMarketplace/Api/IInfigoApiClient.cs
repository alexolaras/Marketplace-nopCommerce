using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

namespace Nop.Plugin.Misc.InfigoMarketplace.Api;

public interface IInfigoApiClient
{
    public Task<IReadOnlyList<PackageApiResponse>> GetPackagesAsync(string search, Guid? categoryId, CancellationToken ct = default);

    public Task<PackageDetailApiResponse> GetPackageAsync(Guid id, CancellationToken ct = default);

    public Task<IReadOnlyList<CategoryApiResponse>> GetCategoriesAsync(string search, CancellationToken ct = default);

    /// <summary>
    /// Issues a low-cost request using ad-hoc credentials. Used by the configure page so admins can
    /// validate before saving.
    /// </summary>
    public Task TestConnectionAsync(string baseUrl, string apiKey, CancellationToken ct = default);
}
