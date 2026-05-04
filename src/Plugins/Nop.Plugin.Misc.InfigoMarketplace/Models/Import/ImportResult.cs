namespace Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

/// <summary>
/// Result of importing one Infigo package. <see cref="ProductId"/> is 0 when <see cref="Action"/> is
/// <see cref="ImportAction.Failed"/>; in that case <see cref="Error"/> carries the reason surfaced to the admin.
/// </summary>
public sealed record ImportResult(
    Guid InfigoId,
    int ProductId,
    ImportAction Action,
    string? Error = null);
