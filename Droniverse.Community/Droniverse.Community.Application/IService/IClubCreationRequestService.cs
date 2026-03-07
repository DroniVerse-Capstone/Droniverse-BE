using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface IClubCreationRequestService
    {
        Task<ClubCreationRequestCreateResponseDto> CreateRequestToCreateClub(ClubCreationRequestCreateDto request);
        Task<ClubCreationRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubCreationRequestUpdateStatusDto request);
        Task<IEnumerable<ClubCreationRequestResponseDto>> GetMyClubCreationRequest(ClubCreationRequestStatus? status = null);
        Task<ClubCreationRequestResponseDto> GetClubCreationRequestById(Guid id);
        Task<ClubCreationRequestUpdateInfoResponseDto> UpdateRequestInfo(Guid id, ClubCreationRequestUpdateInfoDto request);
    }
}
