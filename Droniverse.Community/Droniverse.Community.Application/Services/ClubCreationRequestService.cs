using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class ClubCreationRequestService : IClubCreationRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;

        public ClubCreationRequestService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
        }

        public async Task<ClubCreationRequestCreateResponseDto> CreateRequestToCreateClub(
       ClubCreationRequestCreateDto dto)
        {
            // tạo 
            var requesterId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");

            //var requesterId = _currentUserService.UserId;

            // ===== 2. TODO: Upload image to third-party storage =====
            // string imageUrl = await _fileService.UploadAsync(dto.Image);

            // Fix cứng tạm thời
            string imageUrl = "https://tokyocamera.vn/wp-content/uploads/2021/11/DJI-Mavic-3-Cine-DroneDJ-Featured-Image-1400x700.jpeg";

            var request = new ClubCreationRequest(
                dto.NameVN,
                dto.NameEN,
                dto.DescriptionVN,
                dto.DescriptionEN,
                dto.ClubCode,
                dto.IsPublic,
                dto.LimitParticipant,
                dto.LimitClubManager,
                imageUrl,
                requesterId
            );

            // ===== 4. Save =====
            await _unitOfWork.ClubCreationRequests.Add(request);
            await _unitOfWork.SaveChangeAsync();

            ClubCreationRequestCreateResponseDto response = new()
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameEN = request.NameEN,
                NameVN = request.NameVN,
            };
            return response;
        }

    }
}
