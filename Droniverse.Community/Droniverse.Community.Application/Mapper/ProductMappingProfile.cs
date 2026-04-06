using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductMiniResponseDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductID))
            .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceID));

        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductID))
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.ProductNameVN, opt => opt.MapFrom(src => src.ProductNameVN))
            .ForMember(dest => dest.ProductNameEN, opt => opt.MapFrom(src => src.ProductNameEN))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            ;

        CreateMap<ProductRequestDto, Product>()
            .ForMember(dest => dest.ProductID, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserProducts, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.ProductNameVN, opt => opt.MapFrom(src => src.ProductNameVN))
            .ForMember(dest => dest.ProductNameEN, opt => opt.MapFrom(src => src.ProductNameEN))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            ;
    }
}

