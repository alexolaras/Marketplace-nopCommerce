using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;
using Nop.Plugin.Misc.InfigoMarketplace.Helpers;
using Nop.Plugin.Misc.InfigoMarketplace.Settings;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.InfigoMarketplace.Api;

public class InfigoApiClient(
    HttpClient httpClient,
    ISettingService settingService,
    IStoreContext storeContext)
    : IInfigoApiClient
{
    public async Task<IReadOnlyList<PackageApiResponse>> GetPackagesAsync(string search, Guid? categoryId, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(search))
            query["search"] = search;
        if (categoryId.HasValue)
            query["categoryId"] = categoryId.Value.ToString();

        var path = QueryHelpers.AddQueryString(InfigoMarketplaceDefaults.Endpoints.Packages, query);
        var result = await SendAsync<List<PackageApiResponse>>(HttpMethod.Get, path, baseUrlOverride: null, apiKeyOverride: null, ct);
        return result ?? new List<PackageApiResponse>();
    }

    public async Task<PackageDetailApiResponse> GetPackageAsync(Guid id, CancellationToken ct = default)
    {
        var path = string.Format(InfigoMarketplaceDefaults.Endpoints.PackageById, id);
        return await SendAsync<PackageDetailApiResponse>(HttpMethod.Get, path, baseUrlOverride: null, apiKeyOverride: null, ct);
    }

    public async Task<IReadOnlyList<CategoryApiResponse>> GetCategoriesAsync(string search, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(search))
            query["search"] = search;

        var path = QueryHelpers.AddQueryString(InfigoMarketplaceDefaults.Endpoints.Categories, query);
        var result = await SendAsync<List<CategoryApiResponse>>(HttpMethod.Get, path, baseUrlOverride: null, apiKeyOverride: null, ct);
        return result ?? new List<CategoryApiResponse>();
    }

    public async Task TestConnectionAsync(string baseUrl, string apiKey, CancellationToken ct = default)
    {
        await SendAsync<List<PackageApiResponse>>(HttpMethod.Get, InfigoMarketplaceDefaults.Endpoints.Packages, baseUrl, apiKey, ct);
    }

    public async Task<T> SendAsync<T>(HttpMethod method, string path, string baseUrlOverride, string apiKeyOverride, CancellationToken ct)
    {
        var (baseUrl, apiKey, timeoutSeconds) = await ResolveConnectionAsync(baseUrlOverride, apiKeyOverride);

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InfigoApiException("Base URL is not configured.");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            throw new InfigoApiException($"Base URL '{baseUrl}' is not a valid absolute URI.");

        var requestUri = new Uri(baseUri, path);
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.TryAddWithoutValidation(HeaderNames.Accept, MimeTypes.ApplicationJson);
        request.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, InfigoMarketplaceDefaults.UserAgent);
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.TryAddWithoutValidation(InfigoMarketplaceDefaults.ApiKeyHeader, apiKey);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new InfigoApiException($"Request to Infigo timed out after {timeoutSeconds}s.");
        }
        catch (HttpRequestException ex)
        {
            throw new InfigoApiException($"Could not reach Infigo at {baseUri}: {ex.Message}");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new InfigoApiException(
                    $"Infigo returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                    response.StatusCode,
                    response.ReasonPhrase);

            var body = await response.Content.ReadAsStringAsync(linkedCts.Token);
            if (string.IsNullOrWhiteSpace(body))
                return default;

            try
            {
                return JsonConvert.DeserializeObject<T>(body, JsonHelpers.jsonSettings);
            }
            catch (JsonException ex)
            {
                throw new InfigoApiException($"Could not parse Infigo response: {ex.Message}", response.StatusCode, response.ReasonPhrase);
            }
        }
    }

    public async Task<(string baseUrl, string apiKey, int timeoutSeconds)> ResolveConnectionAsync(string baseUrlOverride, string apiKeyOverride)
    {
        var store = await storeContext.GetCurrentStoreAsync();
        var settings = await settingService.LoadSettingAsync<InfigoMarketplaceSettings>(store.Id);

        var baseUrl = !string.IsNullOrWhiteSpace(baseUrlOverride) ? baseUrlOverride : settings.BaseUrl;
        var apiKey = apiKeyOverride ?? settings.ApiKey;
        var timeoutSeconds = settings.HttpTimeoutSeconds > 0 ? settings.HttpTimeoutSeconds : 30;

        return (baseUrl, apiKey, timeoutSeconds);
    }
}
