using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class LabMappingProfile : Profile
{
    public LabMappingProfile()
    {
        CreateMap<CreateLabRequestDTO, Lab>()
            .ForMember(dest => dest.LabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserLabs, opt => opt.Ignore())
            .ForMember(dest => dest.Reports, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<UpdateLabRequestDTO, Lab>()
            .ForMember(dest => dest.LabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserLabs, opt => opt.Ignore())
            .ForMember(dest => dest.Reports, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<Lab, LabClientViewDTO>();
    }
}
