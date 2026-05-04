using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Data;

namespace Nop.Plugin.Misc.InfigoMarketplace.Services;

public class InstalledPackageTracker(IRepository<GenericAttribute> genericAttributeRepository) : IInstalledPackageTracker
{
    private static readonly string _productKeyGroup = nameof(Product);

    public async Task<IReadOnlyDictionary<Guid, int>> GetInstalledProductIdsAsync(IEnumerable<Guid> packageIds)
    {
        var ids = packageIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        if (ids.Length == 0)
            return new Dictionary<Guid, int>();

        // GenericAttribute.Value is a string; round-trip Guids through ToString("D") to match the import writer.
        var values = ids.Select(id => id.ToString("D")).ToArray();

        var query = from ga in genericAttributeRepository.Table
            where ga.KeyGroup == _productKeyGroup
                  && ga.Key == InfigoMarketplaceDefaults.GenericAttributes.PackageId
                  && values.Contains(ga.Value)
            select new { ga.EntityId, ga.Value };

        var rows = await query.ToListAsync();

        // A package id mapping to multiple products is anomalous — keep the lowest product id (first import wins)
        // so the grid links to a stable target instead of jumping between duplicates.
        return rows
            .Where(r => Guid.TryParse(r.Value, out _))
            .GroupBy(r => Guid.Parse(r.Value))
            .ToDictionary(g => g.Key, g => g.Min(r => r.EntityId));
    }
}
