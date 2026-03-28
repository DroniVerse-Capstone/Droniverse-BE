using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CertificateMappingProfile : Profile
{
    public CertificateMappingProfile()
    {
        CreateMap<CreateCertificateRequestDTO, Certificate>()
            .ForMember(dest => dest.CertificateID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersion, opt => opt.Ignore())
            .ForMember(dest => dest.UserCertificates, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore());

        CreateMap<UpdateCertificateRequestDTO, Certificate>()
            .ForMember(dest => dest.CertificateID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersion, opt => opt.Ignore())
            .ForMember(dest => dest.UserCertificates, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore());

        CreateMap<Certificate, CertificateResponseDTO>();
    }
}
