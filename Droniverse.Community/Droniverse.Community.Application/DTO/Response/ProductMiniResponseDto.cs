using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response;

public record ProductMiniResponseDto(
    Guid ProductId,
    Guid ReferenceId,
    decimal Price,
    CurrencyType Currency,
    ProductStatus Status
);
