using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Factories;
using Nop.Plugin.Misc.InfigoMarketplace.Models;
using Nop.Plugin.Misc.InfigoMarketplace.Services;
using Nop.Plugin.Misc.InfigoMarketplace.Settings;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.InfigoMarketplace.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class InfigoMarketplaceAdminController(
    IInfigoApiClient infigoApiClient,
    IInfigoBrowseModelFactory browseModelFactory,
    IInfigoImportService importService,
    ILocalizationService localizationService,
    INotificationService notificationService,
    ISettingService settingService,
    IStoreContext storeContext)
    : BasePluginController
{
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> Configure()
    {
        var storeId = await storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await settingService.LoadSettingAsync<InfigoMarketplaceSettings>(storeId);

        var model = new ConfigurationModel
        {
            ActiveStoreScopeConfiguration = storeId,
            BaseUrl = settings.BaseUrl,
            ApiKey = settings.ApiKey,
            HttpTimeoutSeconds = settings.HttpTimeoutSeconds == 0 ? 30 : settings.HttpTimeoutSeconds,
        };

        if (storeId > 0)
        {
            model.BaseUrl_OverrideForStore = await settingService.SettingExistsAsync(settings, s => s.BaseUrl, storeId);
            model.ApiKey_OverrideForStore = await settingService.SettingExistsAsync(settings, s => s.ApiKey, storeId);
        }

        return View("~/Plugins/Misc.InfigoMarketplace/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await settingService.LoadSettingAsync<InfigoMarketplaceSettings>(storeId);

        settings.BaseUrl = model.BaseUrl;
        settings.ApiKey = model.ApiKey;
        settings.HttpTimeoutSeconds = model.HttpTimeoutSeconds;

        await settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.BaseUrl, model.BaseUrl_OverrideForStore, storeId, false);
        await settingService.SaveSettingOverridablePerStoreAsync(settings, s => s.ApiKey, model.ApiKey_OverrideForStore, storeId, false);
        await settingService.SaveSettingAsync(settings, s => s.HttpTimeoutSeconds, clearCache: false);
        await settingService.ClearCacheAsync();

        notificationService.SuccessNotification(await localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost]
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> TestConnection(string baseUrl, string apiKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return Json(new
            {
                success = false,
                message = await localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.TestConnection.BaseUrlRequired")
            });

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            return Json(new
            {
                success = false,
                message = await localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.TestConnection.BaseUrlInvalid")
            });

        try
        {
            await infigoApiClient.TestConnectionAsync(baseUrl, apiKey, ct);
            return Json(new
            {
                success = true,
                message = await localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.TestConnection.Success")
            });
        }
        catch (InfigoApiException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> Browse(CancellationToken ct)
    {
        var searchModel = await browseModelFactory.PrepareBrowseSearchModelAsync(new BrowseSearchModel(), ct);

        if (!string.IsNullOrEmpty(searchModel.ApiErrorMessage))
            notificationService.WarningNotification(searchModel.ApiErrorMessage);

        return View("~/Plugins/Misc.InfigoMarketplace/Views/Browse.cshtml", searchModel);
    }

    [HttpPost]
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> BrowsePackageList(BrowseSearchModel searchModel, CancellationToken ct)
    {
        try
        {
            var listModel = await browseModelFactory.PreparePackageListModelAsync(searchModel, ct);
            return Json(listModel);
        }
        catch (InfigoApiException ex)
        {
            return ErrorJson(ex.Message);
        }
    }

    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        try
        {
            var model = await browseModelFactory.PreparePackageDetailsModelAsync(id, ct);
            if (model == null)
                return RedirectToAction(nameof(Browse));

            return View("~/Plugins/Misc.InfigoMarketplace/Views/PackageDetails.cshtml", model);
        }
        catch (InfigoApiException ex)
        {
            notificationService.ErrorNotification(ex.Message);
            return RedirectToAction(nameof(Browse));
        }
    }

    [HttpPost]
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> Import(ICollection<Guid> selectedIds, CancellationToken ct)
    {
        if (selectedIds == null || selectedIds.Count == 0)
            return new StatusCodeResult(StatusCodes.Status204NoContent);

        var summary = await importService.ImportPackagesAsync(selectedIds.ToArray(), ct);

        var template = await localizationService.GetResourceAsync("Plugins.Misc.InfigoMarketplace.Import.Summary");
        var message = string.Format(template, summary.Created, summary.Updated, summary.Failed);

        return Json(new { success = summary.Failed == 0, message });
    }

    [HttpPost]
    [CheckPermission(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE)]
    public async Task<IActionResult> DeleteImportedEntities(CancellationToken ct)
    {
        var (productsDeleted, categoriesDeleted) = await importService.DeleteImportedEntitiesAsync(ct);

        if (productsDeleted == 0 && categoriesDeleted == 0)
        {
            var nothingMsg = await localizationService.GetResourceAsync(
                "Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.NothingToDelete");
            return Json(new { success = true, message = nothingMsg });
        }

        var template = await localizationService.GetResourceAsync(
            "Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.Success");
        return Json(new { success = true, message = string.Format(template, productsDeleted, categoriesDeleted) });
    }
}
