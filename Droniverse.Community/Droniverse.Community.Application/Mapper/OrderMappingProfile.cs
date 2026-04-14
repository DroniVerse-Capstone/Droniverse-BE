using AutoMapper;
using Droniverse.Community.Application.DTO.Response.Mongo;

using Droniverse.Community.Domain.Entities.Mongo;

namespace Droniverse.Community.Application.Mapper;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src._id))
            .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.Item))
            .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            ;

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID))
            .ForMember(dest => dest.ProductNameVN, opt => opt.MapFrom(src => src.ProductNameVN))
            .ForMember(dest => dest.ProductNameEN, opt => opt.MapFrom(src => src.ProductNameEN))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            ;

        CreateMap<PaginationResult<IEnumerable<Order>>, PaginationResult<IEnumerable<OrderResponseDto>>>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));
    }
}

