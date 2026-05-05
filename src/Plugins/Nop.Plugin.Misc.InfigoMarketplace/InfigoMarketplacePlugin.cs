using Nop.Plugin.Misc.InfigoMarketplace.Services;
using Nop.Plugin.Misc.InfigoMarketplace.Settings;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.InfigoMarketplace;

public class InfigoMarketplacePlugin(
    ILocalizationService localizationService,
    IPermissionService permissionService,
    ISettingService settingService,
    IWebHelper webHelper)
    : BasePlugin, IMiscPlugin
{
    public override string GetConfigurationPageUrl()
    {
        return $"{webHelper.GetStoreLocation()}Admin/InfigoMarketplaceAdmin/Configure";
    }

    public override async Task InstallAsync()
    {
        await settingService.SaveSettingAsync(new InfigoMarketplaceSettings
        {
            HttpTimeoutSeconds = 30,
        });

        await permissionService.InsertPermissionsAsync();

        await localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Security.Permission.ManageInfigoMarketplace"] = "Manage Infigo Marketplace",
            ["Plugins.Misc.InfigoMarketplace.Configure.ConnectionSettings"] = "Connection Settings",
            ["Plugins.Misc.InfigoMarketplace.Configure.TestConnection"] = "Test connection",
            ["Plugins.Misc.InfigoMarketplace.Fields.BaseUrl"] = "Infigo base URL",
            ["Plugins.Misc.InfigoMarketplace.Fields.BaseUrl.Hint"] = "The root URL of your Infigo instance (e.g. https://store.example.com).",
            ["Plugins.Misc.InfigoMarketplace.Fields.BaseUrl.Required"] = "Base URL is required.",
            ["Plugins.Misc.InfigoMarketplace.Fields.ApiKey"] = "API key",
            ["Plugins.Misc.InfigoMarketplace.Fields.ApiKey.Hint"] = "Your Infigo API key.",
            ["Plugins.Misc.InfigoMarketplace.Fields.ApiKey.PlaintextWarning"] = "This value is stored as plaintext in the Setting table. Do not assume it is encrypted.",
            ["Plugins.Misc.InfigoMarketplace.Fields.HttpTimeoutSeconds"] = "HTTP timeout (seconds)",
            ["Plugins.Misc.InfigoMarketplace.Fields.HttpTimeoutSeconds.Hint"] = "Timeout in seconds for Infigo API calls.",
            ["Plugins.Misc.InfigoMarketplace.Fields.HttpTimeoutSeconds.Invalid"] = "HTTP timeout must be greater than zero.",
            ["Plugins.Misc.InfigoMarketplace.TestConnection.Success"] = "Connection successful.",
            ["Plugins.Misc.InfigoMarketplace.TestConnection.BaseUrlRequired"] = "Base URL is required.",
            ["Plugins.Misc.InfigoMarketplace.TestConnection.BaseUrlInvalid"] = "Base URL must be a valid absolute URI.",
            ["Plugins.Misc.InfigoMarketplace.Browse.Title"] = "Infigo Marketplace",
            ["Plugins.Misc.InfigoMarketplace.Browse.SearchName"] = "Name",
            ["Plugins.Misc.InfigoMarketplace.Browse.SearchName.Hint"] = "Search packages by name (server-side filter).",
            ["Plugins.Misc.InfigoMarketplace.Browse.SearchCategory"] = "Category",
            ["Plugins.Misc.InfigoMarketplace.Browse.SearchCategory.Hint"] = "Restrict results to a single Infigo category.",
            ["Plugins.Misc.InfigoMarketplace.Browse.SearchCategory.All"] = "All",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Name"] = "Name",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Description"] = "Description",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Type"] = "Type",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Category"] = "Category",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.CurrentVersion"] = "Version",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Tags"] = "Tags",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Installed"] = "Installed",
            ["Plugins.Misc.InfigoMarketplace.Browse.Fields.Action"] = "Action",
            ["Plugins.Misc.InfigoMarketplace.Browse.Import"] = "Import",
            ["Plugins.Misc.InfigoMarketplace.Browse.ReImport"] = "Re-import",
            ["Plugins.Misc.InfigoMarketplace.Browse.ImportSelected"] = "Import selected",
            ["Plugins.Misc.InfigoMarketplace.Browse.ImportSelected.Confirm"] = "Are you sure you want to import the selected packages? Existing imports will be updated in place.",
            ["Plugins.Misc.InfigoMarketplace.Browse.Importing"] = "Importing packages from Infigo, please wait...",
            ["Plugins.Misc.InfigoMarketplace.Import.Summary"] = "Imported {0} new, updated {1}, {2} failed.",
            ["Plugins.Misc.InfigoMarketplace.Configure.DangerZone"] = "Danger Zone",
            ["Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities"] = "Delete all imported entities",
            ["Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.Hint"] = "Permanently deletes all products and categories that were created by the Infigo import and still carry Infigo markers. Admin customisations on those entities will be lost.",
            ["Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.Confirm"] = "This will permanently delete all Infigo-imported products and categories. Admin customisations will be lost. Continue?",
            ["Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.Success"] = "Deleted {0} products and {1} categories.",
            ["Plugins.Misc.InfigoMarketplace.Configure.DeleteImportedEntities.NothingToDelete"] = "No Infigo-imported entities found.",
            ["Plugins.Misc.InfigoMarketplace.Browse.NoItems"] = "No packages found.",
            ["Plugins.Misc.InfigoMarketplace.Browse.Available"] = "Available",
            ["Plugins.Misc.InfigoMarketplace.Browse.Uncategorized"] = "Uncategorized",
            ["Plugins.Misc.InfigoMarketplace.Import.Failed"] = "Import failed. Please try again.",
            ["Plugins.Misc.InfigoMarketplace.Details.Title"] = "Package Details",
            ["Plugins.Misc.InfigoMarketplace.Details.BackToBrowse"] = "Back to browse",
            ["Plugins.Misc.InfigoMarketplace.Details.Category"] = "Category:",
            ["Plugins.Misc.InfigoMarketplace.Details.Type"] = "Type:",
            ["Plugins.Misc.InfigoMarketplace.Details.Tags"] = "Tags",
            ["Plugins.Misc.InfigoMarketplace.Details.NoTagsFound"] = "No Tags Found",
            ["Plugins.Misc.InfigoMarketplace.Details.Latest"] = "(latest)",
            ["Plugins.Misc.InfigoMarketplace.Details.LatestVersion"] = "Latest version",
            ["Plugins.Misc.InfigoMarketplace.Details.About"] = "About this package",
            ["Plugins.Misc.InfigoMarketplace.Details.Details"] = "Details",
            ["Plugins.Misc.InfigoMarketplace.Details.VersionToImport"] = "Version to import",
            ["Plugins.Misc.InfigoMarketplace.Details.ImportPackage"] = "Import package",
            ["Plugins.Misc.InfigoMarketplace.Details.ViewImportedProduct"] = "View imported product",
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await settingService.DeleteSettingAsync<InfigoMarketplaceSettings>();
        await permissionService.DeletePermissionAsync(InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE);
        await localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.InfigoMarketplace");
        await localizationService.DeleteLocaleResourceAsync("Security.Permission.ManageInfigoMarketplace");

        await base.UninstallAsync();
    }
}
