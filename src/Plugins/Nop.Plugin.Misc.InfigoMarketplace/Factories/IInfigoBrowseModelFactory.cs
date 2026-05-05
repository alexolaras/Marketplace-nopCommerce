using Nop.Plugin.Misc.InfigoMarketplace.Models;

namespace Nop.Plugin.Misc.InfigoMarketplace.Factories;

public interface IInfigoBrowseModelFactory
{
    public Task<BrowseSearchModel> PrepareBrowseSearchModelAsync(BrowseSearchModel searchModel, CancellationToken ct = default);

    public Task<PackageListModel> PreparePackageListModelAsync(BrowseSearchModel searchModel, CancellationToken ct = default);

    public Task<PackageDetailsModel> PreparePackageDetailsModelAsync(Guid id, CancellationToken ct = default);
}
