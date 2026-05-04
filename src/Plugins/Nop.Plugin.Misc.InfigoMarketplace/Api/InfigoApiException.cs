using System.Net;

namespace Nop.Plugin.Misc.InfigoMarketplace.Api;

public class InfigoApiException(string message, HttpStatusCode? statusCode = null, string reasonPhrase = null)
    : Exception(message)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
    public string ReasonPhrase { get; } = reasonPhrase;
}
