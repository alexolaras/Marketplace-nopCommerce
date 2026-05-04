using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

/// <summary>
/// Outcome of <see cref="Services.IInfigoProductWriter.UpsertAsync"/>: the persisted nop product and whether it was
/// created from scratch or refreshed from an existing Infigo marker. Aggregated by the import orchestrator to build
/// the per-id <see cref="ImportResult"/>.
/// </summary>
public sealed record ProductUpsertResult(Product Product, ImportAction Action);
