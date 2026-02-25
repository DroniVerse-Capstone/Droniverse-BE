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
            .ForMember(dest => dest.ClubID, opt => opt.MapFrom(src => src.ClubID))
            .ForMember(dest => dest.NameVN, opt => opt.MapFrom(src => src.NameVN))
            .ForMember(dest => dest.NameEN, opt => opt.MapFrom(src => src.NameEN))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            .ForMember(dest => dest.ClubCode, opt => opt.MapFrom(src => src.ClubCode))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
            .ForMember(dest => dest.LimitParticipation, opt => opt.MapFrom(src => src.LimitParticipation))
            .ForMember(dest => dest.LimitClubManagers, opt => opt.MapFrom(src => src.LimitClubManagers))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ClubCategories.Select(cc => cc.Category)))
            .ForMember(dest => dest.Creator, opt => opt.Ignore());

        CreateMap<ClubCreateDto, Club>()
            .ForMember(dest => dest.ClubID, opt => opt.Ignore())
            .ForMember(dest => dest.NameVN, opt => opt.MapFrom(src => src.NameVN))
            .ForMember(dest => dest.NameEN, opt => opt.MapFrom(src => src.NameEN))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            .ForMember(dest => dest.ClubCode, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
            .ForMember(dest => dest.LimitParticipation, opt => opt.MapFrom(src => src.LimitParticipation))
            .ForMember(dest => dest.ClubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.LimitClubManagers, opt => opt.MapFrom(src => src.LimitClubManagers));

        CreateMap<ClubUpdateDto, Club>()
            .ForMember(dest => dest.ClubID, opt => opt.Ignore())
            .ForMember(dest => dest.NameVN, opt => opt.MapFrom(src => src.NameVN))
            .ForMember(dest => dest.NameEN, opt => opt.MapFrom(src => src.NameEN))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.DescriptionEN))
            .ForMember(dest => dest.ClubCode, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
            .ForMember(dest => dest.LimitParticipation, opt => opt.MapFrom(src => src.LimitParticipation))
            .ForMember(dest => dest.ClubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.LimitClubManagers, opt => opt.MapFrom(src => src.LimitClubManagers));
    }
}

