using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface IClubAttemptRequestService
    {
        Task CreateAttemptClubRequest(Guid clubID);
        Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByID(Guid clubID);
        Task<ClubAttemptRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubAttemptRequestUpdateStatusDto request);
        Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByRequester(ClubAttemptRequestStatus? status);
        
        /// <summary>
        /// Lấy tất cả ClubAttemptRequests với filter/search
        /// </summary>
        Task<PaginationResult<IEnumerable<ClubRequestResponseDto>>> GetAllClubAttemptRequests(ClubAttemptRequestSearchRequest searchRequest);
    }
}
