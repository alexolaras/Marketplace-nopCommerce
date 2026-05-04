using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.InfigoMarketplace.Models;

public partial record PackageModel : BaseNopModel
{
    public Guid Id { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Name")]
    public string Name { get; set; }
    
    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Type")]
    public string Type { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Category")]
    public string CategoryName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.CurrentVersion")]
    public string CurrentVersion { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Tags")]
    public string Tags { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.Fields.Installed")]
    public bool Installed { get; set; }

    /// <summary>
    /// nopCommerce <c>Product.Id</c> the package was imported into; null when <see cref="Installed"/> is false.
    /// Drives the row's "edit imported product" link in the grid.
    /// </summary>
    public int? InstalledProductId { get; set; }
}
