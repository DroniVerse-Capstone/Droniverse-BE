using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.Mapper;

public class DashboardMappingProfile : Profile
{
    public DashboardMappingProfile()
    {
        CreateMap<UserResponse, DetailDashboardUserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => string.Join(" ", new[] { src.FirstName, src.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)))))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.TotalSpent, opt => opt.Ignore());
        CreateMap<SimpleUserReponse, DetailDashboardUserResponse>();
    }
}