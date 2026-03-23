using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Application.Mapper;

public class EnrollmentMappingProfile : Profile
{
    public EnrollmentMappingProfile()
    {
        CreateMap<CreateEnrollmentRequestDTO, Enrollment>()
            .ForMember(dest => dest.EnrollmentID, opt => opt.Ignore())
            .ForMember(dest => dest.UserID, opt => opt.Ignore())
            .ForMember(dest => dest.EnrollDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastAccessDate, opt => opt.Ignore())
            .ForMember(dest => dest.Progress, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CourseVersion, opt => opt.Ignore());

        CreateMap<Enrollment, EnrollmentResponseDTO>();
    }
}
