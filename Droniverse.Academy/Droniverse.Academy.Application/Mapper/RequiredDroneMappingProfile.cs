using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class RequiredDroneMappingProfile : Profile
{
    public RequiredDroneMappingProfile()
    {
        CreateMap<RequiredDrone, RequiredDroneResponseDTO>();
        CreateMap<CourseVersion, CourseVersionByDroneClientViewDTO>();
    }
}
