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

        // Mini mappings
        CreateMap<Level, LevelMiniReponse>()
            .ForMember(dest => dest.LevelID, opt => opt.MapFrom(src => src.LevelID))
            .ForMember(dest => dest.LevelNumber, opt => opt.MapFrom(src => src.LevelNumber))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<Drone, DroneMiniReponse>()
            .ForMember(dest => dest.DroneID, opt => opt.MapFrom(src => src.DroneID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DroneNameEN))
            .ForMember(dest => dest.ImgURL, opt => opt.MapFrom(src => src.ImgURL));

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
                        .OrderByDescending(cv => cv.Version)));
    }
}

