using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class DroneTypeMappingProfile : Profile
{
    public DroneTypeMappingProfile()
    {
        CreateMap<CreateDroneTypeRequestDTO, DroneType>()
            .ForMember(dest => dest.DroneTypeID, opt => opt.Ignore())
            .ForMember(dest => dest.Drones, opt => opt.Ignore());

        CreateMap<UpdateDroneTypeRequestDTO, DroneType>()
            .ForMember(dest => dest.DroneTypeID, opt => opt.Ignore())
            .ForMember(dest => dest.Drones, opt => opt.Ignore());

        CreateMap<DroneType, DroneTypeClientViewDTO>();
    }
}
