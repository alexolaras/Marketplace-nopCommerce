using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Plugin.Misc.InfigoMarketplace.Helpers;
using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoProductWriter(
    IProductService productService,
    IUrlRecordService urlRecordService,
    IInfigoMarkerStore markerStore) : IInfigoProductWriter
{
    public async Task<ProductUpsertResult> UpsertAsync(PackageDetailApiResponse pkg)
    {
        var existingProductId = await markerStore.FindMappedProductIdAsync(pkg.Id);

        var (product, action) = existingProductId is null
            ? (await CreateAsync(pkg), ImportAction.Created)
            : (await UpdateAsync(existingProductId.Value, pkg), ImportAction.Updated);

        await markerStore.WriteProductMarkersAsync(
            product,
            pkg.Id,
            pkg.CurrentVersion ?? string.Empty,
            pkg.Type ?? string.Empty);

        return new ProductUpsertResult(product, action);
    }

    private async Task<Product> CreateAsync(PackageDetailApiResponse pkg)
    {
        var now = DateTime.UtcNow;

        var product = new Product
        {
            Name = pkg.Name,
            ShortDescription = CommonHelper.Truncate(pkg.Description, InfigoMarketplaceDefaults.ShortDescriptionMaxLength),
            FullDescription = pkg.Description,
            Sku = string.Format(InfigoMarketplaceDefaults.ImportedProductSkuFormat, pkg.Id),
            ProductType = ProductType.SimpleProduct,
            VisibleIndividually = true,
            Published = true,
            AllowCustomerReviews = true,
            ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            StockQuantity = 1,
            NotifyAdminForQuantityBelow = 1,
            LowStockActivity = LowStockActivity.Nothing,
            BackorderMode = BackorderMode.NoBackorders,
            Price = 0m,
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        };

        await productService.InsertProductAsync(product);

        var seName = await urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
        await urlRecordService.SaveSlugAsync(product, seName, 0);

        return product;
    }

    private async Task<Product> UpdateAsync(int productId, PackageDetailApiResponse pkg)
    {
        var product = await productService.GetProductByIdAsync(productId)
            ?? throw new InvalidOperationException(
                $"Infigo marker references nop product {productId} for package {pkg.Id}, but the product no longer exists.");

        product.Name = pkg.Name;
        product.ShortDescription = CommonHelper.Truncate(pkg.Description, InfigoMarketplaceDefaults.ShortDescriptionMaxLength);
        product.FullDescription = pkg.Description;
        product.UpdatedOnUtc = DateTime.UtcNow;

        await productService.UpdateProductAsync(product);
        return product;
    }
}
