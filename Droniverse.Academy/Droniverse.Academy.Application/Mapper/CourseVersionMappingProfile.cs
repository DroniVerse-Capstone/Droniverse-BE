using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Mapper
{
    public class CourseVersionMappingProfile : Profile
    {
        public CourseVersionMappingProfile()
        {
            CreateMap<CreateCourseVersionRequestDTO, CourseVersion>()
                .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
                .ForMember(dest => dest.CourseID, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.CourseVersionCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Modules, opt => opt.Ignore())
                .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredDrones, opt => opt.Ignore())
                .ForMember(dest => dest.Feedbacks, opt => opt.Ignore());

            CreateMap<CourseVersion, CourseVersionResponseDTO>()
                .ForMember(dest => dest.ChangeLog, opt => opt.MapFrom(src => src.ChangeLog))
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredDrones, opt => opt.Ignore());

            CreateMap<CourseVersion, CourseVersion>()
                .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
                .ForMember(dest => dest.CourseID, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
                .ForMember(dest => dest.Modules, opt => opt.Ignore())
                .ForMember(dest => dest.CourseVersionCategories, opt => opt.Ignore())
                .ForMember(dest => dest.Feedbacks, opt => opt.Ignore())
                .ForMember(dest => dest.RequiredDrones, opt => opt.Ignore())
                .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
                .ForMember(dest => dest.Certificate, opt => opt.Ignore());
        }
    }
}
