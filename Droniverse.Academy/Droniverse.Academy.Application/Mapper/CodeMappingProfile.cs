
using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class CodeMappingProfile : Profile
{
    public CodeMappingProfile()
    {
        CreateMap<Code, CodeResponseDTO>()
            .ForMember(dest => dest.CodeID, opt => opt.MapFrom(src => src.CodeID))
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.Club, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerUser, opt => opt.Ignore())
            .ForMember(dest => dest.UsedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.UsedDate, opt => opt.MapFrom(src => src.UsedDate))
            .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

    }
}

