using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.Mapper;

public class ClubMappingProfile : Profile
{
    public ClubMappingProfile()
    {
        //  Club
        CreateMap<Club, ClubResponseDto>()
            .ForMember(dest => dest.ClubPolicy, opt => opt.MapFrom(src => src.ClubPolicy));

        CreateMap<ClubCreateDto, Club>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionVN));

        CreateMap<ClubUpdateDto, Club>();

        // ClubPolicy
        CreateMap<ClubPolicyCreateDto, ClubPolicy>();

        CreateMap<ClubPolicy, ClubPolicyResponseDto>();
    }
}

