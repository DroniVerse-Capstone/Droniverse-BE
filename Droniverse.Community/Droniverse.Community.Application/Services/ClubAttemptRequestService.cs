using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class ClubAttemptRequestService : IClubAttemptRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;

        public ClubAttemptRequestService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
        }

        public async Task CreateAttemptClubRequest(Guid requesterID, Guid clubID)
        {

            Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID);
            if (club == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy club với ID {clubID}");
            }

            ClubAttemptRequest clubRequest = new(clubID, requesterID);
            await _unitOfWork.ClubAttemptRequests.Add(clubRequest);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByID(Guid clubID)
        {
            if (clubID == Guid.Empty)
                throw new ArgumentException("ClubID không được để trống", nameof(clubID));

            var clubRequests = await _unitOfWork.ClubAttemptRequests
                .GetManyByCondition(
                    c => c.ClubID == clubID,
                    c => c.Club
                );

            if (clubRequests == null)
                throw new KeyNotFoundException();

            if (!clubRequests.Any())
                return Enumerable.Empty<ClubRequestResponseDto>();

            var userIds = clubRequests
                .SelectMany(x => new[] { x.RequesterID, x.ApproverID })
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var users = await _identityMicroserviceClient.GetUsersBulk((IEnumerable<Guid>)userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => u);
            var result = clubRequests.Select(clubRequest =>
            {
                userDict.TryGetValue(clubRequest.RequesterID, out var requester);
                UserResponse approver = null;
                if (clubRequest.ApproverID != null)
                    userDict.TryGetValue((Guid)clubRequest.ApproverID, out approver);

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.Club?.NameVN,
                    clubRequest.Club?.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    clubRequest.Status,
                    clubRequest.CreateAt,
                    clubRequest.ProcessedAt
                );
            });

            return result;
        }

    }
}
