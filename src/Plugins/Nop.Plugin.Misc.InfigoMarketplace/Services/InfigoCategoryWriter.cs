using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoCategoryWriter(
    ICategoryService categoryService,
    IUrlRecordService urlRecordService,
    IInfigoMarkerStore markerStore) : IInfigoCategoryWriter
{
    private readonly Dictionary<Guid, int> _nopCategoryIdCache = new();

    public async Task SyncProductCategoryAsync(Product product, PackageDetailApiResponse pkg)
    {
        var existingMappings = await categoryService.GetProductCategoriesByProductIdAsync(product.Id, showHidden: true);

        if (pkg.CategoryId is null || string.IsNullOrWhiteSpace(pkg.CategoryName))
        {
            await PruneInfigoSourcedMappingsAsync(existingMappings, exceptCategoryId: null);
            return;
        }

        var nopCategoryId = await EnsureCategoryAsync(pkg.CategoryId.Value, pkg.CategoryName);
        await PruneInfigoSourcedMappingsAsync(existingMappings, exceptCategoryId: nopCategoryId);

        if (existingMappings.All(pc => pc.CategoryId != nopCategoryId))
        {
            await categoryService.InsertProductCategoryAsync(new ProductCategory
            {
                ProductId = product.Id,
                CategoryId = nopCategoryId,
                DisplayOrder = 1
            });
        }
    }

    private async Task<int> EnsureCategoryAsync(Guid infigoCategoryId, string name)
    {
        if (_nopCategoryIdCache.TryGetValue(infigoCategoryId, out var cached))
            return cached;

        var existingId = await markerStore.FindMappedCategoryIdAsync(infigoCategoryId);
        if (existingId is not null && await categoryService.GetCategoryByIdAsync(existingId.Value) is { } existing)
        {
            if (!string.Equals(existing.Name, name, StringComparison.Ordinal))
            {
                existing.Name = name;
                existing.UpdatedOnUtc = DateTime.UtcNow;
                await categoryService.UpdateCategoryAsync(existing);
            }

            _nopCategoryIdCache[infigoCategoryId] = existing.Id;
            return existing.Id;
        }

        var created = await CreateCategoryAsync(name);
        await markerStore.WriteCategoryMarkerAsync(created, infigoCategoryId);

        _nopCategoryIdCache[infigoCategoryId] = created.Id;
        return created.Id;
    }

    private async Task<Category> CreateCategoryAsync(string name)
    {
        var now = DateTime.UtcNow;
        var category = new Category
        {
            Name = name,
            Published = true,
            ShowOnHomepage = false,
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        };
        await categoryService.InsertCategoryAsync(category);

        var seName = await urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
        await urlRecordService.SaveSlugAsync(category, seName, 0);

        return category;
    }

    private async Task PruneInfigoSourcedMappingsAsync(IList<ProductCategory> mappings, int? exceptCategoryId)
    {
        var candidates = mappings.Where(pc => pc.CategoryId != exceptCategoryId).ToList();
        if (candidates.Count == 0)
            return;

        var infigoSourced = await markerStore.GetInfigoSourcedCategoryIdsAsync(candidates.Select(pc => pc.CategoryId));
        foreach (var pc in candidates.Where(pc => infigoSourced.Contains(pc.CategoryId)))
            await categoryService.DeleteProductCategoryAsync(pc);
    }
}
