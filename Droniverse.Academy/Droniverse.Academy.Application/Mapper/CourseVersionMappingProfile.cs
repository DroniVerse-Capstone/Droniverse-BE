using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Mapper
{
    public class CourseVersionMappingProfile : Profile
    {
        public CourseVersionMappingProfile()
        {
            CreateMap<CourseVersion, CourseVersionResponseDTO>();
        }
    }
}
