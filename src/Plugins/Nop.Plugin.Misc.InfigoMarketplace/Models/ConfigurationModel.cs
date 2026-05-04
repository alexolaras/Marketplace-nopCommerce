using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.InfigoMarketplace.Models;

public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Fields.BaseUrl")]
    public string BaseUrl { get; set; }
    public bool BaseUrl_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Fields.ApiKey")]
    public string ApiKey { get; set; }
    public bool ApiKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.InfigoMarketplace.Fields.HttpTimeoutSeconds")]
    public int HttpTimeoutSeconds { get; set; }
}
