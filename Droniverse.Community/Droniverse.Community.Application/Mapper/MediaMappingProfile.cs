using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class MediaMappingProfile : Profile
{
    public MediaMappingProfile()
    {
        //  Media
        CreateMap<Media, MediaResponseDto>()
            .ForMember(dest => dest.MediaTypeName, opt => opt.MapFrom(src => src.MediaType.TypeNameEN));

    }
}

