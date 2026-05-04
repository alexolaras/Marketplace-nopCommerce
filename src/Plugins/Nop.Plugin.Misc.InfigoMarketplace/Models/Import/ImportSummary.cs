namespace Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

/// <summary>
/// Aggregate outcome of a bulk import. Counts are derived from <see cref="Details"/> so the controller can render
/// a single toast ("Imported N new, updated M, K failed") without re-walking the list.
/// </summary>
public sealed record ImportSummary(
    int Created,
    int Updated,
    int Failed,
    IReadOnlyList<ImportResult> Details);
