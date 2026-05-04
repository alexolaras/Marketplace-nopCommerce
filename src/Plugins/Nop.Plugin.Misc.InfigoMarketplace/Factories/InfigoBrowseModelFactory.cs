using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
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
            // Page should still render so the admin can see the error and fix the connection.
            searchModel.ApiErrorMessage = ex.Message;
        }

        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<PackageListModel> PreparePackageListModelAsync(BrowseSearchModel searchModel, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var packages = await apiClient.GetPackagesAsync(searchModel.SearchName, searchModel.SearchCategoryId, ct);
        var filtered = packages.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        var pagedPackages = filtered.ToPagedList(searchModel);

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
                InstalledProductId = installedMap.TryGetValue(package.Id, out var productId) ? productId : null
            }));

        return listModel;
    }
}
