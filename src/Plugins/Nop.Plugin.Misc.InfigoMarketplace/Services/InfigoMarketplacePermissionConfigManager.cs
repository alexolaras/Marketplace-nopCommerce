using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InfigoMarketplacePermissionConfigManager : IPermissionConfigManager
{
    public const string MANAGE_INFIGO_MARKETPLACE = "ManageInfigoMarketplace";

    public IList<PermissionConfig> AllConfigs =>
        new List<PermissionConfig>
        {
            new("Manage Infigo Marketplace", MANAGE_INFIGO_MARKETPLACE, "Configuration",
                NopCustomerDefaults.AdministratorsRoleName)
        };
}
