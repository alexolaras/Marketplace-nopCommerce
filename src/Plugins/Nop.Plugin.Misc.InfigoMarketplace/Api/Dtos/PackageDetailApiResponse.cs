namespace Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

public record PackageDetailApiResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    Guid? CategoryId,
    string? CategoryName,
    string CurrentVersion,
    List<string> Tags,
    string? DownloadUrl
);
