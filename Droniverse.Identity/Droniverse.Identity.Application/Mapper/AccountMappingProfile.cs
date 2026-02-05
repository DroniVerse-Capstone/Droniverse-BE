using AutoMapper;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.Mapper;
internal class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<RegisterDto, Account>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.UserInfo, opt => opt.MapFrom(src => new UserInfo
            {
                FirstName = src.FirstName,
                LastName = src.LastName,
                DateOfBirth = src.DateOfBirth,
                Phone = src.Phone
            }));

        CreateMap<Account, UserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserInfo.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserInfo.LastName))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.UserInfo.DateOfBirth));
    }
}

