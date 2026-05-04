using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Misc.InfigoMarketplace.Api;
using Nop.Plugin.Misc.InfigoMarketplace.Settings;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.InfigoMarketplace.Helpers;

public static class InfigoApiHelper
{
    public static async Task<T> SendAsync<T>(
        HttpClient httpClient,
        HttpMethod method,
        string path,
        string baseUrl,
        string apiKey,
        int timeoutSeconds,
        CancellationToken ct)
    {
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

    public static async Task<(string baseUrl, string apiKey, int timeoutSeconds)> ResolveConnectionAsync(
        ISettingService settingService,
        IStoreContext storeContext,
        string baseUrlOverride,
        string apiKeyOverride)
    {
        var store = await storeContext.GetCurrentStoreAsync();
        var settings = await settingService.LoadSettingAsync<InfigoMarketplaceSettings>(store.Id);

        var baseUrl = !string.IsNullOrWhiteSpace(baseUrlOverride) ? baseUrlOverride : settings.BaseUrl;
        var apiKey = apiKeyOverride ?? settings.ApiKey;
        var timeoutSeconds = settings.HttpTimeoutSeconds > 0 ? settings.HttpTimeoutSeconds : 30;

        return (baseUrl, apiKey, timeoutSeconds);
    }
}
