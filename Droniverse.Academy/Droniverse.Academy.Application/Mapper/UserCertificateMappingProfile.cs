using Droniverse.Academy.Application.DTO.Request;
using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserCertificateMappingProfile : Profile
{
    public UserCertificateMappingProfile()
    {
        CreateMap<GrantUserCertificateRequestDTO, UserCertificate>()
            .ForMember(dest => dest.CertificateID, opt => opt.MapFrom(src => src.CertificateID))
            .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.CertificateUrl, opt => opt.Ignore())
            .ForMember(dest => dest.AchievedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Certificate, opt => opt.Ignore());

        CreateMap<UserCertificate, UserCertificateResponseDTO>();

        CreateMap<Certificate, CertificateResponseDTO>();
    }
}
