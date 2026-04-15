using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class RequiredDroneMappingProfile : Profile
{
    public RequiredDroneMappingProfile()
    {
        CreateMap<RequiredDrone, RequiredDroneResponseDTO>();
        CreateMap<RequiredDrone, DroneClientViewDTO>()
            .ForMember(dest => dest.DroneID, opt => opt.MapFrom(src => src.Drone.DroneID))
            .ForMember(dest => dest.DroneTypeID, opt => opt.MapFrom(src => src.Drone.DroneTypeID))
            .ForMember(dest => dest.DroneTypeNameVN, opt => opt.MapFrom(src => src.Drone.DroneType != null ? src.Drone.DroneType.TypeNameVN : string.Empty))
            .ForMember(dest => dest.DroneTypeNameEN, opt => opt.MapFrom(src => src.Drone.DroneType != null ? src.Drone.DroneType.TypeNameEN : string.Empty))
            .ForMember(dest => dest.DroneNameVN, opt => opt.MapFrom(src => src.Drone.DroneNameVN))
            .ForMember(dest => dest.DroneNameEN, opt => opt.MapFrom(src => src.Drone.DroneNameEN))
            .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Drone.Manufacturer))
            .ForMember(dest => dest.DescriptionVN, opt => opt.MapFrom(src => src.Drone.DescriptionVN))
            .ForMember(dest => dest.DescriptionEN, opt => opt.MapFrom(src => src.Drone.DescriptionEN))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Drone.Height))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Drone.Weight))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Drone.Status))
            .ForMember(dest => dest.Model3DLink, opt => opt.MapFrom(src => src.Drone.Model3DLink));
        CreateMap<CourseVersion, CourseVersionByDroneClientViewDTO>();
    }
}
