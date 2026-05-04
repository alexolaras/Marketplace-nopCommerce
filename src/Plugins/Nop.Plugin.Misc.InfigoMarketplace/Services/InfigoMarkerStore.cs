using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoMarkerStore(
    IRepository<GenericAttribute> genericAttributeRepository,
    IGenericAttributeService genericAttributeService) : IInfigoMarkerStore
{
    public Task<int?> FindMappedProductIdAsync(Guid infigoPackageId)
    {
        return FindEntityIdByMarkerAsync(nameof(Product), InfigoMarketplaceDefaults.GenericAttributes.PackageId, infigoPackageId);
    }

    public Task<int?> FindMappedCategoryIdAsync(Guid infigoCategoryId)
    {
        return FindEntityIdByMarkerAsync(nameof(Category), InfigoMarketplaceDefaults.GenericAttributes.CategoryId, infigoCategoryId);
    }

    public async Task<HashSet<int>> GetInfigoSourcedCategoryIdsAsync(IEnumerable<int> categoryIds)
    {
        var ids = categoryIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new HashSet<int>();

        var query = from ga in genericAttributeRepository.Table
            where ga.KeyGroup == nameof(Category)
                  && ga.Key == InfigoMarketplaceDefaults.GenericAttributes.CategoryId
                  && ids.Contains(ga.EntityId)
            select ga.EntityId;

        var found = await query.ToListAsync();
        return found.ToHashSet();
    }

    public async Task WriteProductMarkersAsync(Product product, Guid packageId, string packageVersion, string packageType)
    {
        await genericAttributeService.SaveAttributeAsync(product, InfigoMarketplaceDefaults.GenericAttributes.PackageId, packageId.ToString("D"));
        await genericAttributeService.SaveAttributeAsync(product, InfigoMarketplaceDefaults.GenericAttributes.PackageVersion, packageVersion);
        await genericAttributeService.SaveAttributeAsync(product, InfigoMarketplaceDefaults.GenericAttributes.PackageType, packageType);
    }

    public Task WriteCategoryMarkerAsync(Category category, Guid infigoCategoryId) =>
        genericAttributeService.SaveAttributeAsync(category, InfigoMarketplaceDefaults.GenericAttributes.CategoryId, infigoCategoryId.ToString("D"));

    public async Task<IReadOnlyList<int>> GetAllMarkedProductIdsAsync()
    {
        return await GetAllEntityIdsByMarkerAsync(nameof(Product), InfigoMarketplaceDefaults.GenericAttributes.PackageId);
    }

    public async Task<IReadOnlyList<int>> GetAllMarkedCategoryIdsAsync()
    {
        return await GetAllEntityIdsByMarkerAsync(nameof(Category), InfigoMarketplaceDefaults.GenericAttributes.CategoryId);
    }

    public async Task DeleteAllMarkersAsync()
    {
        await genericAttributeRepository.DeleteAsync(ga =>
            (ga.KeyGroup == nameof(Product)
                 && (ga.Key == InfigoMarketplaceDefaults.GenericAttributes.PackageId
                     || ga.Key == InfigoMarketplaceDefaults.GenericAttributes.PackageVersion
                     || ga.Key == InfigoMarketplaceDefaults.GenericAttributes.PackageType))
            || (ga.KeyGroup == nameof(Category)
                 && ga.Key == InfigoMarketplaceDefaults.GenericAttributes.CategoryId));
    }

    private async Task<IReadOnlyList<int>> GetAllEntityIdsByMarkerAsync(string keyGroup, string key)
    {
        var query = from ga in genericAttributeRepository.Table
            where ga.KeyGroup == keyGroup && ga.Key == key
            select ga.EntityId;

        return await query.Distinct().ToListAsync();
    }

    private async Task<int?> FindEntityIdByMarkerAsync(string keyGroup, string key, Guid infigoId)
    {
        var value = infigoId.ToString("D");
        var query = from ga in genericAttributeRepository.Table
            where ga.KeyGroup == keyGroup
                  && ga.Key == key
                  && ga.Value == value
            orderby ga.EntityId
            select ga.EntityId;

        var ids = await query.ToListAsync();
        return ids.Count == 0 ? null : ids[0];
    }
}
