using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<CreateModuleRequestDTO, Module>()
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersion, opt => opt.Ignore())
            .ForMember(dest => dest.UserModules, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<UpdateModuleRequestDTO, Module>()
            .ForMember(dest => dest.ModuleID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersionID, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersion, opt => opt.Ignore())
            .ForMember(dest => dest.UserModules, opt => opt.Ignore())
            .ForMember(dest => dest.Lessons, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<Module, ModuleResponseDTO>();
        CreateMap<Module, ModuleClientViewDTO>();
    }
}
