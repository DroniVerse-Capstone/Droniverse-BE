using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<CourseVersionCategory, CategoryResponseDTO>()
            .ForCtorParam("CategoryID", opt => opt.MapFrom(src => src.CategoryID));
    }
}
