using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Plugin.Misc.InfigoMarketplace.Helpers;
using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoProductWriter(
    IProductService productService,
    IUrlRecordService urlRecordService,
    IPictureService pictureService,
    IDownloadService downloadService,
    IInfigoImageDownloader imageDownloader,
    IInfigoFileDownloader fileDownloader,
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
            pkg.Versions?.FirstOrDefault() ?? string.Empty,
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

        await AttachPicturesAsync(product, pkg);
        await AttachDownloadAsync(product, pkg);

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

        await AttachDownloadAsync(product, pkg);

        return product;
    }

    private async Task AttachPicturesAsync(Product product, PackageDetailApiResponse pkg)
    {
        if (pkg.Images is null || pkg.Images.Count == 0)
            return;

        var seoFilenameBase = await pictureService.GetPictureSeNameAsync(product.Name);
        var displayOrder = 0;

        foreach (var image in pkg.Images)
        {
            if (string.IsNullOrWhiteSpace(image.Url))
                continue;

            var downloaded = await imageDownloader.TryDownloadAsync(image.Url);
            if (downloaded is null)
                continue;

            var picture = await pictureService.InsertPictureAsync(
                downloaded.Bytes,
                downloaded.MimeType,
                seoFilenameBase,
                altAttribute: product.Name,
                titleAttribute: product.Name);

            await productService.InsertProductPictureAsync(new ProductPicture
            {
                ProductId = product.Id,
                PictureId = picture.Id,
                DisplayOrder = displayOrder++
            });
        }
    }

    private async Task AttachDownloadAsync(Product product, PackageDetailApiResponse pkg)
    {
        if (string.IsNullOrWhiteSpace(pkg.DownloadUrl))
            return;

        var file = await fileDownloader.TryDownloadAsync(pkg.DownloadUrl);
        if (file is null)
            return;

        if (product.DownloadId > 0)
        {
            var existing = await downloadService.GetDownloadByIdAsync(product.DownloadId);
            if (existing is not null)
                await downloadService.DeleteDownloadAsync(existing);
        }

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadBinary = file.Bytes,
            ContentType = file.ContentType,
            Filename = Path.GetFileNameWithoutExtension(file.FileName),
            Extension = file.Extension,
            IsNew = true
        };
        await downloadService.InsertDownloadAsync(download);

        product.IsDownload = true;
        product.DownloadId = download.Id;
        product.UnlimitedDownloads = true;
        product.DownloadActivationTypeId = (int)DownloadActivationType.WhenOrderIsPaid;
        await productService.UpdateProductAsync(product);
    }
}
