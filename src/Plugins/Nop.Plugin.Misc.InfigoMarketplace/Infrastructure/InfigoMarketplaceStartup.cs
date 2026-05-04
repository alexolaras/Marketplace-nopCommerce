using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Factories;
using Nop.Plugin.Misc.InfigoMarketplace.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.InfigoMarketplace.Infrastructure;

public class InfigoMarketplaceStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var builder = services.AddHttpClient<IInfigoApiClient, InfigoApiClient>();
        builder.AddStandardResilienceHandler();
        builder.WithProxy();

        services.AddScoped<IInstalledPackageTracker, InstalledPackageTracker>();
        services.AddScoped<IInfigoMarkerStore, InfigoMarkerStore>();
        services.AddScoped<IInfigoProductWriter, InfigoProductWriter>();
        services.AddScoped<IInfigoCategoryWriter, InfigoCategoryWriter>();
        services.AddScoped<IInfigoImportService, InfigoImportService>();
        services.AddScoped<IInfigoBrowseModelFactory, InfigoBrowseModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
