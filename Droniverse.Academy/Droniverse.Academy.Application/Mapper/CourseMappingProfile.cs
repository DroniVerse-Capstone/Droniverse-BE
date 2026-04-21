using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using System.Linq;

namespace Droniverse.Academy.Application.Mapper;

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<ProductMiniResponseDTO, ProductMiniResponseDTO>();
        CreateMap<Level, LevelMiniResponse>();



        CreateMap<Course, CourseResponseDTO>()
            .ForMember(dest => dest.CurrentVersion,
                opt => opt.MapFrom(src => src.CurrentVersion != null ? src.CurrentVersion : null))
            .ForMember(dest => dest.Level,
                opt => opt.MapFrom(src => src.Level != null ? src.Level : null))
            .ForMember(dest => dest.Drone,
                opt => opt.MapFrom(src => src.Drone != null ? src.Drone : null));

       
        CreateMap<Course, CourseDetailResponseDTO>()
            .ForMember(dest => dest.CurrentVersion,
                opt => opt.MapFrom(src => src.CurrentVersion))
            .ForMember(dest => dest.CourseVersions,
                opt => opt.MapFrom(src =>
                    src.CourseVersions
                        .OrderByDescending(cv => cv.Version)))
            .ForMember(dest => dest.Level,
                opt => opt.MapFrom(src => src.Level != null ? src.Level : null))
            .ForMember(dest => dest.Drone,
                opt => opt.MapFrom(src => src.Drone != null ? src.Drone : null)); ;
    }
}

