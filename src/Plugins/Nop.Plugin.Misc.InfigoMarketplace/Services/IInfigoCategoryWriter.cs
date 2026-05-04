using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Owns the side of the import that touches nop Categories: ensuring an Infigo category exists as a nop category,
/// and keeping the product's <see cref="ProductCategory"/> rows in sync without trampling admin-added ones.
/// Caches Infigo→nop category id resolutions for the lifetime of the request, so a bulk import won't race two
/// packages in the same Infigo category to insert duplicate nop categories.
/// </summary>
public interface IInfigoCategoryWriter
{
    /// <summary>
    /// Aligns the product's category mapping with the Infigo package. Admin-attached categories survive untouched;
    /// only Infigo-sourced rows are pruned when the package's category changes or is cleared.
    /// </summary>
    Task SyncProductCategoryAsync(Product product, PackageDetailApiResponse pkg);
}
