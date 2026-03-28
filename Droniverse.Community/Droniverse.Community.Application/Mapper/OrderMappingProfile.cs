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
            .ForMember(dest => dest.Item, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            ;

        CreateMap<OrderItem, OrderItemDto>();
    }
}

