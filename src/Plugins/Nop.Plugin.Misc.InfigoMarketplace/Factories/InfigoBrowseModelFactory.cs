using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Plugin.Misc.InfigoMarketplace.Models;
using Nop.Plugin.Misc.InfigoMarketplace.Services;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.InfigoMarketplace.Factories;

public class InfigoBrowseModelFactory(
    IInfigoApiClient apiClient,
    IInstalledPackageTracker installedPackageTracker,
    IHtmlFormatter htmlFormatter,
    ILocalizationService localizationService) : IInfigoBrowseModelFactory
{
    public async Task<BrowseSearchModel> PrepareBrowseSearchModelAsync(BrowseSearchModel searchModel, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.AvailableCategories.Add(new SelectListItem
        {
            Text = await localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.Browse.SearchCategory.All"),
            Value = string.Empty
        });

        try
        {
            var categories = await apiClient.GetCategoriesAsync(search: null, ct);
            foreach (var category in categories.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                searchModel.AvailableCategories.Add(new SelectListItem
                {
                    Text = category.Name,
                    Value = category.Id.ToString()
                });
            }
        }
        catch (InfigoApiException ex)
        {
            searchModel.ApiErrorMessage = ex.Message;
        }

        //searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<PackageListModel> PreparePackageListModelAsync(BrowseSearchModel searchModel, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var response = await apiClient.GetPackagesAsync(
            searchModel.SearchName, searchModel.SearchCategoryId,
            searchModel.Page, searchModel.PageSize, ct);

        var pagedPackages = new PagedList<PackageApiResponse>(
            response.Items.ToList(), searchModel.Page - 1, searchModel.PageSize, response.TotalCount);

        var installedMap = await installedPackageTracker.GetInstalledProductIdsAsync(pagedPackages.Select(p => p.Id));

        var listModel = new PackageListModel().PrepareToGrid(searchModel, pagedPackages, () =>
            pagedPackages.Select(package => new PackageModel
            {
                Id = package.Id,
                Name = package.Name,
                Description = htmlFormatter.StripTags(package.Description),
                Type = package.Type,
                CategoryName = package.CategoryName,
                CurrentVersion = package.CurrentVersion,
                Tags = package.Tags is { Count: > 0 } ? string.Join(", ", package.Tags) : string.Empty,
                Installed = installedMap.ContainsKey(package.Id),
                InstalledProductId = installedMap.TryGetValue(package.Id, out var productId) ? productId : null,
                ImageUrl = package.Images?.FirstOrDefault()?.Thumbnails?.FirstOrDefault()?.Url
                    ?? package.Images?.FirstOrDefault()?.Url
            }));

        return listModel;
    }

    public async Task<PackageDetailsModel> PreparePackageDetailsModelAsync(Guid id, CancellationToken ct = default)
    {
        var package = await apiClient.GetPackageAsync(id, ct);
        if (package == null)
            return null;

        var installedMap = await installedPackageTracker.GetInstalledProductIdsAsync(new[] { package.Id });

        var model = new PackageDetailsModel
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            Type = package.Type,
            CategoryName = package.CategoryName,
            CurrentVersion = package.CurrentVersion,
            Versions = package.Versions ?? new List<string>(),
            Tags = package.Tags ?? new List<string>(),
            Installed = installedMap.ContainsKey(package.Id),
            InstalledProductId = installedMap.TryGetValue(package.Id, out var productId) ? productId : null,
        };

        if (package.Images is { Count: > 0 })
        {
            foreach (var image in package.Images)
            {
                var fullUrl = image.Url;
                var thumbUrl = image.Thumbnails?.FirstOrDefault()?.Url ?? fullUrl;
                if (string.IsNullOrWhiteSpace(fullUrl) && string.IsNullOrWhiteSpace(thumbUrl))
                    continue;

                model.Images.Add(new PackageImageModel
                {
                    Url = fullUrl ?? thumbUrl,
                    ThumbnailUrl = thumbUrl ?? fullUrl,
                });
            }
        }

        return model;
    }
}
