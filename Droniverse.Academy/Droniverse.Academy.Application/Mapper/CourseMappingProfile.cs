using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {

        CreateMap<Course, CourseResponseDTO>()
            .ForMember(dest => dest.courseVersion,
                opt => opt.MapFrom(src =>
                    src.CourseVersions.FirstOrDefault()));

        CreateMap<Course, CourseDetailResponseDTO>()
            .ForMember(dest => dest.courseVersions,
                opt => opt.MapFrom(src =>
                    src.CourseVersions
                        .OrderByDescending(cv => cv.Version)));
    }
}

