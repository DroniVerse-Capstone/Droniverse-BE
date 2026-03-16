using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class DroneMappingProfile : Profile
{
    public DroneMappingProfile()
    {
        CreateMap<CreateDroneRequestDTO, Drone>()
            .ForMember(dest => dest.DroneID, opt => opt.Ignore())
            .ForMember(dest => dest.DroneTypeID, opt => opt.Ignore())
            .ForMember(dest => dest.DroneType, opt => opt.Ignore())
            .ForMember(dest => dest.RequiredDrones, opt => opt.Ignore());

        CreateMap<UpdateDroneRequestDTO, Drone>()
            .ForMember(dest => dest.DroneID, opt => opt.Ignore())
            .ForMember(dest => dest.DroneType, opt => opt.Ignore())
            .ForMember(dest => dest.RequiredDrones, opt => opt.Ignore());

        CreateMap<Drone, DroneClientViewDTO>()
            .ForMember(dest => dest.DroneTypeNameVN, opt => opt.MapFrom(src => src.DroneType != null ? src.DroneType.TypeNameVN : string.Empty))
            .ForMember(dest => dest.DroneTypeNameEN, opt => opt.MapFrom(src => src.DroneType != null ? src.DroneType.TypeNameEN : string.Empty));
    }
}
