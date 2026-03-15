using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class UserCertificateMappingProfile : Profile
{
    public UserCertificateMappingProfile()
    {
        CreateMap<UserCertificate, UserCertificateResponseDTO>();

        CreateMap<Certificate, CertificateResponseDTO>();
    }
}
