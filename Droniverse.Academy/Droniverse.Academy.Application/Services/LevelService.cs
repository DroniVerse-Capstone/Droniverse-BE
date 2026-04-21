using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Services
{
    public class LevelService : ILevelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LevelService> _logger;
        private readonly IMapper _mapper;

        public LevelService(IUnitOfWork unitOfWork, ILogger<LevelService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<IEnumerable<LevelMiniResponse>> GetLevelByDroneAsync(Guid droneId)
        {
            var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.DroneID == droneId,
            orderBy: q => q.OrderBy(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);
            return _mapper.Map<IEnumerable<LevelMiniResponse>>(levels.Data);
        }
    }
}
