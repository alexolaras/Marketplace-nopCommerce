using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.InfigoMarketplace.Models;

public partial record PackageDetailsModel : BaseNopModel
{
    public PackageDetailsModel()
    {
        Tags = new List<string>();
        Images = new List<PackageImageModel>();
        Versions = new List<string>();
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public string CategoryName { get; set; }

    /// <summary>
    /// All available versions of this package, ordered as returned by the Infigo API
    /// (latest first). The first entry is treated as the latest version.
    /// </summary>
    public IList<string> Versions { get; set; }

    public IList<string> Tags { get; set; }

    public IList<PackageImageModel> Images { get; set; }

    /// <summary>
    /// True if the package has already been imported as a nop Product. Drives the Re-import label
    /// and the "View imported product" link.
    /// </summary>
    public bool Installed { get; set; }

    public int? InstalledProductId { get; set; }
}

public partial record PackageImageModel : BaseNopModel
{
    public string Url { get; set; }

    public string ThumbnailUrl { get; set; }
}
