namespace Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

public record ImageApiResponse(
    Guid Id,
    int Width,
    int Height,
    string? Url,
    List<ImageApiResponse> Thumbnails
);
