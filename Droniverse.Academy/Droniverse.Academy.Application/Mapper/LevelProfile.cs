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
    public class LevelProfile : Profile
    {
        public LevelProfile()
        {
            CreateMap<Level, LevelMiniResponse>()
                .ForMember(dest => dest.LevelID, opt => opt.MapFrom(src => src.LevelID))
                .ForMember(dest => dest.LevelNumber, opt => opt.MapFrom(src => src.LevelNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
