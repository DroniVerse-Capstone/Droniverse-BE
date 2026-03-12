using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserCertificateMappingProfile : Profile
{
    public UserCertificateMappingProfile()
    {
        CreateMap<UserCertificate, UserCertificateResponseDTO>()
            .ForCtorParam("CertificateID", opt => opt.MapFrom(src => src.CertificateID))
            .ForCtorParam("UserID", opt => opt.MapFrom(src => src.UserID))
            .ForCtorParam("SerialNumber", opt => opt.MapFrom(src => src.SerialNumber))
            .ForCtorParam("AchievedDate", opt => opt.MapFrom(src => src.AchievedDate))
            .ForCtorParam("Status", opt => opt.MapFrom(src => src.Status))
            .ForCtorParam("Certificate", opt => opt.MapFrom(src => src.Certificate));

        CreateMap<Certificate, CertificateResponseDTO>();
    }
}
