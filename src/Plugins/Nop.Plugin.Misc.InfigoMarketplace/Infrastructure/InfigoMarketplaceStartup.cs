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

        // Presigned image URLs target the storage backend, not the Infigo API host, so use a separate typed client.
        var imageBuilder = services.AddHttpClient<IInfigoImageDownloader, InfigoImageDownloader>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(30);
        });
        imageBuilder.WithProxy();

        // Package binaries can be substantially larger than images, so use a separate client with a longer timeout.
        var fileBuilder = services.AddHttpClient<IInfigoFileDownloader, InfigoFileDownloader>(c =>
        {
            c.Timeout = TimeSpan.FromMinutes(5);
        });
        fileBuilder.WithProxy();

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
