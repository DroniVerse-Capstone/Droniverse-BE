using AutoMapper;
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
            CreateMap<CourseVersion, CourseVersionResponseDTO>()
                .ForCtorParam("Categories", opt => opt.MapFrom(src => src.CourseVersionCategories))
                .ForCtorParam("RequiredDrones", opt => opt.MapFrom(src => src.RequiredDrones));

            CreateMap<CourseVersionCategory, CategoryResponseDTO>()
                .ConstructUsing(c => new CategoryResponseDTO(c.CategoryID));

            CreateMap<RequiredDrone, RequiredDroneResponseDTO>()
                .ConstructUsing(r => new RequiredDroneResponseDTO(r.CourseVersionID, r.DroneID));

            CreateMap<Certificate, CertificateResponseDTO>()
                .ForCtorParam("CertificateID", opt => opt.MapFrom(src => src.CertificateID))
                .ForCtorParam("CourseVersionID", opt => opt.MapFrom(src => src.CourseVersionID))
                .ForCtorParam("CertificateName", opt => opt.MapFrom(src => src.CertificateName))
                .ForCtorParam("ImageUrl", opt => opt.MapFrom(src => src.ImageUrl))
                .ForCtorParam("LogoCertificate", opt => opt.MapFrom(src => src.LogoCertificate))
                .ForCtorParam("Description", opt => opt.MapFrom(src => src.Description))
                .ForCtorParam("Signature", opt => opt.MapFrom(src => src.Signature))
                .ForCtorParam("AuthorName", opt => opt.MapFrom(src => src.AuthorName))
                .ForCtorParam("CreateAt", opt => opt.MapFrom(src => src.CreateAt))
                .ForCtorParam("CreateBy", opt => opt.MapFrom(src => src.CreateBy))
                .ForCtorParam("UpdateAt", opt => opt.MapFrom(src => src.UpdateAt))
                .ForCtorParam("UpdateBy", opt => opt.MapFrom(src => src.UpdateBy));
        }
    }
}
