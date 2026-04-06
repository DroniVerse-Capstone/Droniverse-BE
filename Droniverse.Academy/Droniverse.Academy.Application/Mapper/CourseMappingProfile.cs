using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<ProductMiniResponseDTO, ProductMiniResponseDTO>();

        CreateMap<Course, CourseResponseDTO>()
            .ForMember(dest => dest.CurrentVersion,
                opt => opt.MapFrom(src =>
                    src.CurrentVersion != null ? src.CurrentVersion : null));
       
        CreateMap<Course, CourseDetailResponseDTO>()
            .ForMember(dest => dest.CurrentVersion,
                opt => opt.MapFrom(src => src.CurrentVersion))
            .ForMember(dest => dest.CourseVersions,
                opt => opt.MapFrom(src =>
                    src.CourseVersions
                        .OrderByDescending(cv => cv.Version)));
    }
}

