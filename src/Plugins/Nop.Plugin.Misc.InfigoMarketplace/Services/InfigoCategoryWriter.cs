using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoCategoryWriter(
    ICategoryService categoryService,
    IUrlRecordService urlRecordService,
    IPictureService pictureService,
    IInfigoImageDownloader imageDownloader,
    IInfigoApiClient apiClient,
    IInfigoMarkerStore markerStore) : IInfigoCategoryWriter
{
    private readonly Dictionary<Guid, int> _nopCategoryIdCache = new();
    private Dictionary<Guid, ImageApiResponse?>? _categoryImagesByInfigoId;

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

        var image = await ResolveCategoryImageAsync(infigoCategoryId);
        var created = await CreateCategoryAsync(name, image);
        await markerStore.WriteCategoryMarkerAsync(created, infigoCategoryId);

        _nopCategoryIdCache[infigoCategoryId] = created.Id;
        return created.Id;
    }

    private async Task<Category> CreateCategoryAsync(string name, ImageApiResponse? image)
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

        if (image is not null && !string.IsNullOrWhiteSpace(image.Url))
        {
            var downloaded = await imageDownloader.TryDownloadAsync(image.Url);
            if (downloaded is not null)
            {
                var seoFilename = await pictureService.GetPictureSeNameAsync(name);
                var picture = await pictureService.InsertPictureAsync(
                    downloaded.Bytes,
                    downloaded.MimeType,
                    seoFilename,
                    altAttribute: name,
                    titleAttribute: name);
                category.PictureId = picture.Id;
            }
        }

        await categoryService.InsertCategoryAsync(category);

        var seName = await urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
        await urlRecordService.SaveSlugAsync(category, seName, 0);

        return category;
    }

    private async Task<ImageApiResponse?> ResolveCategoryImageAsync(Guid infigoCategoryId)
    {
        // PackageDetailApiResponse carries only CategoryId+Name, so when we're about to create a new nop category we
        // fall back to /categories to pick up its image. Cached for the lifetime of the request (writer is scoped).
        if (_categoryImagesByInfigoId is null)
        {
            var categories = await apiClient.GetCategoriesAsync(string.Empty);
            _categoryImagesByInfigoId = categories.ToDictionary(c => c.Id, c => c.Image);
        }

        return _categoryImagesByInfigoId.TryGetValue(infigoCategoryId, out var image) ? image : null;
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
