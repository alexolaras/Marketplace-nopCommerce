using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.InfigoMarketplace.Models;

public partial record BrowseSearchModel : BaseSearchModel
{
    public BrowseSearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Browse.SearchCategory")]
    public Guid? SearchCategoryId { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    /// <summary>
    /// Surfaces a non-fatal error from the Infigo API (e.g. categories couldn't be loaded) so the
    /// Browse view can render anyway with a notice instead of a 500.
    /// </summary>
    public string ApiErrorMessage { get; set; }
}
