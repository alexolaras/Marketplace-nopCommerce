using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Single boundary for the <c>InfigoMarketplace.*</c> <see cref="Nop.Core.Domain.Common.GenericAttribute"/> markers.
/// Reading a marker answers "is this entity Infigo-sourced and which Infigo id does it correspond to?"; writing one
/// stamps that identity onto a freshly created or updated nop entity.
/// </summary>
public interface IInfigoMarkerStore
{
    /// <summary>
    /// Returns the nop product id previously linked to <paramref name="infigoPackageId"/>, or <c>null</c> when no
    /// product carries the marker.
    /// </summary>
    Task<int?> FindMappedProductIdAsync(Guid infigoPackageId);

    /// <summary>
    /// Returns the nop category id previously linked to <paramref name="infigoCategoryId"/>, or <c>null</c> when no
    /// category carries the marker.
    /// </summary>
    Task<int?> FindMappedCategoryIdAsync(Guid infigoCategoryId);

    /// <summary>
    /// Of the supplied nop category ids, returns those that carry the Infigo category marker. Used to distinguish
    /// admin-attached <see cref="ProductCategory"/> rows (preserve) from Infigo-sourced ones (safe to prune).
    /// </summary>
    Task<HashSet<int>> GetInfigoSourcedCategoryIdsAsync(IEnumerable<int> categoryIds);

    /// <summary>
    /// Stamps the package id, version and type markers onto a product so subsequent imports recognise it.
    /// </summary>
    Task WriteProductMarkersAsync(Product product, Guid packageId, string packageVersion, string packageType);

    /// <summary>
    /// Stamps the Infigo category id marker onto a freshly created nop category.
    /// </summary>
    Task WriteCategoryMarkerAsync(Category category, Guid infigoCategoryId);

    /// <summary>
    /// Returns the nop product ids for every product that carries the Infigo package-id marker.
    /// </summary>
    Task<IReadOnlyList<int>> GetAllMarkedProductIdsAsync();

    /// <summary>
    /// Returns the nop category ids for every category that carries the Infigo category-id marker.
    /// </summary>
    Task<IReadOnlyList<int>> GetAllMarkedCategoryIdsAsync();

    /// <summary>
    /// Removes every Infigo marker from the GenericAttribute table. Pairs with a purge of imported entities so that
    /// nopCommerce's soft-delete doesn't leave behind markers that keep advertising the (now-deleted) products and
    /// categories as already installed.
    /// </summary>
    Task DeleteAllMarkersAsync();
}
