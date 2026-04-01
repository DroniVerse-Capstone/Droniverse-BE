using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class ProductCategoryMappingProfile : Profile
{
    public ProductCategoryMappingProfile()
    {
        CreateMap<ProductCategory, ProductCategoryResponseDto>()
            .ForMember(dest => dest.CategoryNameVN, opt => opt.MapFrom(src => src.CategoryNameVN))
            .ForMember(dest => dest.CategoryNameEN, opt => opt.MapFrom(src => src.CategoryNameEN))
            .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
            .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => src.UpdateAt))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryID))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            ;

        CreateMap<ProductCategoryRequestDto, ProductCategory>()
            .ForMember(dest => dest.CategoryNameVN, opt => opt.MapFrom(src => src.CategoryNameVN))
            .ForMember(dest => dest.CategoryNameEN, opt => opt.MapFrom(src => src.CategoryNameEN))
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryID, opt => opt.Ignore())
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            ;
    }
}

