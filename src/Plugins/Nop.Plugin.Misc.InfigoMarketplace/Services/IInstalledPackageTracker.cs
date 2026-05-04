namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Resolves which Infigo packages have already been imported into nopCommerce as Products.
/// Reads <see cref="Nop.Core.Domain.Common.GenericAttribute"/> rows written by the import job.
/// </summary>
public interface IInstalledPackageTracker
{
    /// <summary>
    /// For the supplied Infigo package ids, returns a map from package id to the imported Product id.
    /// Packages absent from the map are not installed.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetInstalledProductIdsAsync(IEnumerable<Guid> packageIds);
}
