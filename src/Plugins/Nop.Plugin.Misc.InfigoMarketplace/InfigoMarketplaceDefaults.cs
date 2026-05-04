namespace Nop.Plugin.Misc.InfigoMarketplace;

public static class InfigoMarketplaceDefaults
{
    public const string SystemName = "Misc.InfigoMarketplace";

    public const string UserAgent = "nopCommerce-InfigoMarketplace";

    /// <summary>
    /// Header name expected by Infigo's <c>ApiKeyHandler</c> middleware. Verify against
    /// <c>Presentation/Middleware/ApiKeyHandler.cs</c> in the Infigo solution before shipping.
    /// </summary>
    public const string ApiKeyHeader = "X-Api-Key";

    public static class Endpoints
    {
        public const string Packages = "api/v1/packages";
        public const string PackageById = "api/v1/packages/{0}";
        public const string Categories = "api/v1/categories";
    }

    /// <summary>
    /// <see cref="Nop.Core.Domain.Common.GenericAttribute"/> keys written against Product / Category entities to
    /// track which Infigo package or category a nop entity was imported from. Reads from these keys back the
    /// Browse grid's "Installed" column and let the import job stay idempotent without an extra table.
    /// </summary>
    public static class GenericAttributes
    {
        public const string PackageId = "InfigoMarketplace.PackageId";
        public const string PackageVersion = "InfigoMarketplace.PackageVersion";
        public const string PackageType = "InfigoMarketplace.PackageType";
        public const string CategoryId = "InfigoMarketplace.CategoryId";
    }

    /// <summary>
    /// Deterministic SKU template for products imported from Infigo. The compact ("N") Guid format keeps the SKU
    /// short while remaining collision-free within the plugin's namespace.
    /// </summary>
    public const string ImportedProductSkuFormat = "infigo-{0:N}";

    /// <summary>
    /// Maximum length of <see cref="Nop.Core.Domain.Catalog.Product.ShortDescription"/>. Infigo package descriptions
    /// can be substantially longer than the storefront teaser slot, so the import job truncates here.
    /// </summary>
    public const int ShortDescriptionMaxLength = 500;
}
