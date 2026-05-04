using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.InfigoMarketplace.Settings;

public class InfigoMarketplaceSettings : ISettings
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public int HttpTimeoutSeconds { get; set; }
}
