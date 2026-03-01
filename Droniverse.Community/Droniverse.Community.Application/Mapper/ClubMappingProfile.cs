using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class ClubMappingProfile : Profile
{
    public ClubMappingProfile()
    {
        CreateMap<Club, ClubResponseDto>();
        CreateMap<ClubCreateDto, Club>();
        CreateMap<ClubUpdateDto, Club>();
    }
}

