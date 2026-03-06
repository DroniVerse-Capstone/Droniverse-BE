using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Mapper
{
    public class ClubRequestMappingProfile : Profile
    {
        public ClubRequestMappingProfile()
        {
            CreateMap<ClubAttemptRequestCreateDto, ClubAttemptRequest>();
            CreateMap<ClubAttemptRequest, ClubRequestResponseDto>();
        }
    }
}
