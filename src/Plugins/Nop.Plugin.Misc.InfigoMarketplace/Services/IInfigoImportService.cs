using Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

/// <summary>
/// Imports Infigo packages into nopCommerce as Products. Idempotent: re-importing the same package id updates the
/// existing product in place rather than creating a duplicate. Identity is tracked via
/// <see cref="Nop.Core.Domain.Common.GenericAttribute"/> rows — see
/// <see cref="InfigoMarketplaceDefaults.GenericAttributes"/>.
/// </summary>
public interface IInfigoImportService
{
    /// <summary>
    /// Imports a single Infigo package. Safe to call repeatedly with the same id; failures are reported in the
    /// returned <see cref="ImportResult"/> rather than thrown (cancellation is the only exception that propagates).
    /// </summary>
    public Task<ImportResult> ImportPackageAsync(Guid infigoPackageId, CancellationToken ct = default);

    /// <summary>
    /// Imports a batch of Infigo packages, continuing past per-item failures so a single bad package doesn't abort
    /// the whole job. The returned summary reports both totals and per-id detail.
    /// </summary>
    public Task<ImportSummary> ImportPackagesAsync(IReadOnlyCollection<Guid> infigoPackageIds, CancellationToken ct = default);

    /// <summary>
    /// Deletes every product and category that still carries an Infigo marker. Entities customised by admins but no
    /// longer marked are left untouched. Returns the count of deleted products and categories.
    /// </summary>
    public Task<(int ProductsDeleted, int CategoriesDeleted)> DeleteImportedEntitiesAsync(CancellationToken ct = default);
}
