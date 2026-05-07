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
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.Description));

        CreateMap<ClubCreateDto, Club>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionVN));

        CreateMap<ClubUpdateDto, Club>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ManagerID, opt => opt.Ignore())
            .ForMember(dest => dest.DroneID, opt => opt.Ignore())
            .ForMember(dest => dest.ClubCode, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.SuspendedReason, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.Participations, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.Competitions, opt => opt.Ignore())
            .ForMember(dest => dest.ClubRequests, opt => opt.Ignore());

    }
}

