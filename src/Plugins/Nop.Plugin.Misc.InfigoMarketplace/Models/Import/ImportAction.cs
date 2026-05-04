namespace Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

/// <summary>
/// Outcome of importing a single Infigo package as a nopCommerce product.
/// </summary>
public enum ImportAction
{
    Created,
    Updated,
    Failed
}
