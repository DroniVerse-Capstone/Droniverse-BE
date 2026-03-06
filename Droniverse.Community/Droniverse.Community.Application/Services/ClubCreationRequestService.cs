using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
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

            var request = new ClubCreationRequest(
                dto.NameVN,
                dto.NameEN,
                dto.Description,
                dto.IsPublic,
                dto.LimitParticipant,
                dto.LimitClubManager,
                dto.Image,
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

        public async Task<ClubCreationRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubCreationRequestUpdateStatusDto dto)
        {
            // Temporary: fix ApproverId
            var approverId = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee");
            
            // Get the request from database
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(r => r.ClubCreationRequestID == id);
            
            if (request == null)
                throw new InvalidOperationException($"Club creation request with ID {id} not found.");

            // Update status based on the requested status
            switch (dto.Status)
            {
                case ClubCreationRequestStatus.APPROVED:
                    // Create new Club with ACTIVE status
                    var newClub = new Club(
                        request.NameVN,
                        request.NameEN,
                        request.Description,
                        GenerateClubCode(),
                        request.IsPublic,
                        request.LimitParticipant,
                        request.LimitClubManager,
                        request.RequesterID
                    );

                    // Add club to database
                    await _unitOfWork.Clubs.Add(newClub);
                    await _unitOfWork.SaveChangeAsync();

                    // Approve request with the created club's ID
                    request.Approve(approverId, newClub.ClubID);
                    break;

                case ClubCreationRequestStatus.REJECTED:
                    if (string.IsNullOrWhiteSpace(dto.RejectReason))
                        throw new ArgumentException("Reject reason is required for rejection.");
                    request.Reject(approverId, dto.RejectReason);
                    break;

                case ClubCreationRequestStatus.CANCEL:
                    request.Cancel(request.RequesterID);
                    break;

                default:
                    throw new InvalidOperationException($"Cannot update to status {dto.Status}.");
            }

            // Save changes
            await _unitOfWork.ClubCreationRequests.Update(request);
            await _unitOfWork.SaveChangeAsync();

            // Return response
            var response = new ClubCreationRequestUpdateStatusResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameVN = request.NameVN,
                NameEN = request.NameEN,
                Status = request.Status,
                UpdatedAt = request.UpdatedAt ?? DateTime.UtcNow,
                RejectReason = request.RejectReason,
                ClubID = request.ClubID
            };

            return response;
        }

        /// <summary>
        /// Generate a unique 6-character club code
        /// </summary>
        private string GenerateClubCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 6)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}
