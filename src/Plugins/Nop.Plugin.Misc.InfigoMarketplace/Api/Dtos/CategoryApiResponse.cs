namespace Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

public record CategoryApiResponse(
    Guid Id,
    string Name,
    Guid? ParentId
);
