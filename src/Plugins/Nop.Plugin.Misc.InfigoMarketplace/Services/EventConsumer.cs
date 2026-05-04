using Nop.Services.Helpers;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class EventConsumer(IPluginManager<IPlugin> pluginManager, IWebHelper webHelper)
    : BaseAdminMenuCreatedEventConsumer(pluginManager)
{
    protected override Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
    {
        var descriptor = plugin?.PluginDescriptor;
        if (descriptor == null)
            return Task.FromResult<AdminMenuItem>(new AdminMenuItem { Visible = false });

        var item = new AdminMenuItem
        {
            SystemName = descriptor.SystemName,
            Title = "Infigo Marketplace",
            IconClass = "far fa-dot-circle",
            Url = $"{webHelper.GetStoreLocation()}Admin/InfigoMarketplaceAdmin/Browse",
            PermissionNames = new[] { InfigoMarketplacePermissionConfigManager.MANAGE_INFIGO_MARKETPLACE }
        };

        return Task.FromResult(item);
    }

    protected override string PluginSystemName => "Misc.InfigoMarketplace";

    protected override MenuItemInsertType InsertType => MenuItemInsertType.After;

    // "Product tags" is a direct child of "Catalog" — inserting after it places the
    // new node as a sibling under Catalog. AdminMenuItem.Insert recurses into children.
    protected override string AfterMenuSystemName => "Product tags";
}
