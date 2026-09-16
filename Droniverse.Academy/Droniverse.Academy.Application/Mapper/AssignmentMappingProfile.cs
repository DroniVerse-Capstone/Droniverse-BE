using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class AssignmentMappingProfile : Profile
{
    public AssignmentMappingProfile()
    {
        CreateMap<CreateAssignmentRequestDTO, Assignment>()
            .ForMember(dest => dest.AssignmentID, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<UpdateAssignmentRequestDTO, Assignment>()
            .ForMember(dest => dest.AssignmentID, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateAt, opt => opt.Ignore());

        CreateMap<Assignment, AssignmentClientViewDTO>();
    }
}
