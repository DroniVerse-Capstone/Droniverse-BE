using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response.Mongo;

public record OrderItemDto(
    Guid ProductID, 
    string ProductName, 
    ProductType Type, 
    double UnitOfPrice, 
    int Quantity, 
    double Total)
{}