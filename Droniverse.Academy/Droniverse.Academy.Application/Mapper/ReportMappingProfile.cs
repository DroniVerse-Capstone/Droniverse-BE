using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class ReportMappingProfile : Profile
{
    public ReportMappingProfile()
    {
        CreateMap<CreateReportRequestDTO, Report>()
            .ForMember(dest => dest.ReportID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseVN, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseEN, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore());

        CreateMap<UpdateReportRequestDTO, Report>()
            .ForMember(dest => dest.ReportID, opt => opt.Ignore())
            .ForMember(dest => dest.LabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseVN, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseEN, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore());

        CreateMap<RespondReportRequestDTO, Report>()
            .ForMember(dest => dest.ReportID, opt => opt.Ignore())
            .ForMember(dest => dest.LabID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.Ignore())
            .ForMember(dest => dest.Lab, opt => opt.Ignore());

        CreateMap<Report, ReportResponseDTO>();
    }
}
