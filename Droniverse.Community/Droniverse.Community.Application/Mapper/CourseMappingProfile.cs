using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;
public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        //CreateMap<Course, CourseResponse>()
        //    .ForMember(dest => dest.CourseID, opt => opt.MapFrom(src => src.CourseID))
        //    .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
        //    .ForMember(dest => dest.TitleEN, opt => opt.Ignore())
        //    .ForMember(dest => dest.TitleVN, opt => opt.Ignore());
    }
}

