using AutoMapper;
using Droniverse.Community.Application.DTO.Response.Mongo;

using Droniverse.Community.Domain.Entities.Mongo;

namespace Droniverse.Community.Application.Mapper;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionID))
            .ForMember(dest => dest.PaymentUrl, opt => opt.MapFrom(src => src.PaymentUrl))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
            .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
            
            ;
    }
}

