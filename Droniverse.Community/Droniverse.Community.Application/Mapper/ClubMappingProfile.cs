using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class ClubMappingProfile : Profile
{
    public ClubMappingProfile()
    {
        CreateMap<Club, ClubResponseDto>()
            .ForMember(dest => dest.Categories,
                opt => opt.MapFrom(src => src.ClubCategories.Select(cc => cc.Category)));

        CreateMap<ClubCreateDto, Club>();
        CreateMap<ClubUpdateDto, Club>();
    }
}

