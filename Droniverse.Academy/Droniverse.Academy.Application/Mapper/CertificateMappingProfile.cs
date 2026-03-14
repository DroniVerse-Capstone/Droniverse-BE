using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CertificateMappingProfile : Profile
{
    public CertificateMappingProfile()
    {
        CreateMap<Certificate, CertificateResponseDTO>();
    }
}
