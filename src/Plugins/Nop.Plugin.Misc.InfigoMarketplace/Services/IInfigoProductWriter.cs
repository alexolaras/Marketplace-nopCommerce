using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Owns the product side of the import: deciding between create and update via the Infigo marker, persisting the
/// nop <see cref="Nop.Core.Domain.Catalog.Product"/>, and stamping the Infigo identity markers back onto it.
/// Update path is deliberately conservative — admin-edited fields (Price, Published, slug, pictures, SEO) are
/// preserved across re-imports.
/// </summary>
public interface IInfigoProductWriter
{
    Task<ProductUpsertResult> UpsertAsync(PackageDetailApiResponse pkg);
}
