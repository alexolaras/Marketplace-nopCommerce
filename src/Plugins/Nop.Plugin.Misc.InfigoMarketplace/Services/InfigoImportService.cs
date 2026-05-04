using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Orchestrates the import of Infigo packages into nopCommerce. Each collaborator owns one concern: product upsert
/// (<see cref="IInfigoProductWriter"/>), category mapping (<see cref="IInfigoCategoryWriter"/>) and tag sync
/// (<see cref="IProductTagService"/>). Identity markers are written by the writers themselves so the orchestrator
/// stays purely about sequencing and per-item failure isolation.
/// </summary>
public class InfigoImportService(
    IInfigoApiClient apiClient,
    IInfigoProductWriter productWriter,
    IInfigoCategoryWriter categoryWriter,
    IProductTagService productTagService,
    IInfigoMarkerStore markerStore,
    IProductService productService,
    ICategoryService categoryService,
    ILogger logger) : IInfigoImportService
{
    public async Task<ImportResult> ImportPackageAsync(Guid infigoPackageId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var pkg = await apiClient.GetPackageAsync(infigoPackageId, ct);

            var upsert = await productWriter.UpsertAsync(pkg);
            await categoryWriter.SyncProductCategoryAsync(upsert.Product, pkg);
            await productTagService.UpdateProductTagsAsync(upsert.Product, (pkg.Tags ?? new List<string>()).ToArray());

            await logger.InformationAsync(
                $"Infigo import: {upsert.Action.ToString().ToLowerInvariant()} product {upsert.Product.Id} from package {pkg.Id} ({pkg.Name})");

            return new ImportResult(pkg.Id, upsert.Product.Id, upsert.Action);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync($"Infigo import: failed to import package {infigoPackageId}", ex);
            return new ImportResult(infigoPackageId, ProductId: 0, ImportAction.Failed, ex.Message);
        }
    }

    public async Task<ImportSummary> ImportPackagesAsync(IReadOnlyCollection<Guid> infigoPackageIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(infigoPackageIds);

        var details = new List<ImportResult>(infigoPackageIds.Count);
        foreach (var id in infigoPackageIds.Distinct())
            details.Add(await ImportPackageAsync(id, ct));

        return new ImportSummary(
            Created: details.Count(r => r.Action == ImportAction.Created),
            Updated: details.Count(r => r.Action == ImportAction.Updated),
            Failed: details.Count(r => r.Action == ImportAction.Failed),
            Details: details);
    }

    public async Task<(int ProductsDeleted, int CategoriesDeleted)> DeleteImportedEntitiesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var productIds = await markerStore.GetAllMarkedProductIdsAsync();
        var productsDeleted = 0;
        foreach (var id in productIds)
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product != null && !product.Deleted)
            {
                await productService.DeleteProductAsync(product);
                productsDeleted++;
            }
        }

        var categoryIds = await markerStore.GetAllMarkedCategoryIdsAsync();
        var categoriesDeleted = 0;
        foreach (var id in categoryIds)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category != null && !category.Deleted)
            {
                await categoryService.DeleteCategoryAsync(category);
                categoriesDeleted++;
            }
        }

        // nopCommerce soft-deletes products and categories, so the GA marker rows survive. Wipe them too — otherwise
        // the Browse grid would still flag these packages as installed and offer "Re-import" instead of "Install".
        await markerStore.DeleteAllMarkersAsync();

        await logger.InformationAsync(
            $"Infigo purge: deleted {productsDeleted} products and {categoriesDeleted} categories.");

        return (productsDeleted, categoriesDeleted);
    }
}
