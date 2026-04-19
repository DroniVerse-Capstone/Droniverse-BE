
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
            .ForMember(dest => dest.CourseID, opt => opt.MapFrom(src => src.CourseID))
            .ForMember(dest => dest.ClubID, opt => opt.MapFrom(src => src.ClubID))
            .ForMember(dest => dest.OwnedUserID, opt => opt.MapFrom(_ => (Guid?)null))
            .ForMember(dest => dest.UsedByUserID, opt => opt.MapFrom(src => src.UsedByUserID))
            .ForMember(dest => dest.UsedDate, opt => opt.MapFrom(src => src.UsedDate))
            .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

    }
}

