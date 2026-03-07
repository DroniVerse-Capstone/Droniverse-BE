using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services;
internal class ClubService : IClubService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    public ClubService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient, AcademyMicroserviceClient academyMicroserviceClient)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _identityMicroserviceClient = identityMicroserviceClient;
        _academyMicroserviceClient = academyMicroserviceClient;
    }

    public async Task<ClubResponseDto> CreateClub(ClubCreateDto clubRequestDto)
    {
        if (clubRequestDto == null)
        {
            throw new ArgumentNullException(nameof(clubRequestDto), "Club request data cannot be null.");
        }

        if (clubRequestDto.CategoryIDs != null && clubRequestDto.CategoryIDs.Any())
        {
            foreach (var categoryId in clubRequestDto.CategoryIDs)
            {
                var category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == categoryId);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
                }
            }
        }

        Club club = _mapper.Map<Club>(clubRequestDto);
        club.ClubID = Guid.NewGuid();
        club.ClubCode = GenerateClubCode();

        UserResponse user = null;
        try
        {
            user = await _identityMicroserviceClient.GetUserByUserID(clubRequestDto.CreatedBy);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {clubRequestDto.CreatedBy} not found.");
        }
        club.CreatedBy = user.UserId;

        await _unitOfWork.Clubs.Add(club);
        await _unitOfWork.SaveChangeAsync();

        if (clubRequestDto.CategoryIDs != null && clubRequestDto.CategoryIDs.Any())
        {
            foreach (var categoryId in clubRequestDto.CategoryIDs)
            {
                var clubCategory = new ClubCategory
                {
                    ClubID = club.ClubID,
                    CategoryID = categoryId
                };
                await _unitOfWork.ClubCategories.Add(clubCategory);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        var createdClub = await _unitOfWork.Clubs.GetByIdWithCategories(club.ClubID);

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(createdClub);
        response = response with { Creator = user };
        return response;
    }

    private static string GenerateClubCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
        var random = new Random();

        return new string(
            Enumerable.Range(0, length)
                      .Select(_ => chars[random.Next(chars.Length)])
                      .ToArray()
        );
    }

    public async Task<bool> DeleteClub(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        try
        {
            await _unitOfWork.Clubs.Delete(club);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<IEnumerable<ClubResponseDto>> GetAllClubs()
    {
        IEnumerable<Club> clubList = await _unitOfWork.Clubs.GetAllWithCategories();
        return await MapClubsWithStats(clubList);
    }

    public async Task<ClubResponseDto> GetClubById(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByIdWithCategories(id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }

        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds(new[] { id });
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds(new[] { id });

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        response.TotalMembers = memberCounts.GetValueOrDefault(id, 0);
        response.TotalCourses = courseCounts.GetValueOrDefault(id, 0);
        return response;
    }

    public async Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto clubUpdateDto)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        _mapper.Map(clubUpdateDto, club);
        await _unitOfWork.Clubs.Update(club);
        await _unitOfWork.SaveChangeAsync();

        var updatedClub = await _unitOfWork.Clubs.GetByIdWithCategories(id);
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(updatedClub);
        return response;
    }

    public async Task<JoinClubResponse> JoinClub(ClubJoinDto request)
    {
        var club = await _unitOfWork.Clubs
            .GetByCondition(c => c.ClubCode == request.clubCode);

        if (club == null)
            throw new KeyNotFoundException($"Club with club code {request.clubCode} not found.");

        Guid currentUserId = Guid.Parse("3197734d-d25d-42b1-b968-84b6ee4d33c2"); // member

        bool isUserExisted = await _unitOfWork.Participations.IsUserInClub(club.ClubID, currentUserId);

        if (isUserExisted)
            throw new InvalidOperationException($"Thành viên này đã là thuộc câu lạc bộ [{club.NameVN}]");

        JoinClubResponse response = new()
        {
            ClubID = club.ClubID,
            NameEN = club.NameEN,
            NameVN = club.NameVN,
            ClubIsPublic = club.IsPublic,
        };

        if (club.IsPublic)
        {
            var participation = new Participation(
                currentUserId,
                club.ClubID,
                null
            );

            await _unitOfWork.Participations.Add(participation);
        }
        else
        {
            var user = _unitOfWork.ClubAttemptRequests.IsUserInClubAttemptRequest(currentUserId, club.ClubID);

            if (user != null)
                throw new InvalidOperationException("Yêu cầu tham gia club của người dùng này đang chờ được duyệt !");

            var clubAttemptRequest = new ClubAttemptRequest(
                currentUserId,
                club.ClubID
            );

            await _unitOfWork.ClubAttemptRequests.Add(clubAttemptRequest);
            response.ClubAttemptRequestID = clubAttemptRequest.ClubRequestID;
        }

        await _unitOfWork.SaveChangeAsync();

        return response;
    }

    public async Task<PaginationResult<IEnumerable<UserResponse>>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {clubID} not found.");
        }

        var participations = await _unitOfWork.Participations.GetManyByCondition(
            p => p.ClubID == clubID && p.Status == Domain.Enums.ParticipationStatus.ACTIVE
        );

        var userIds = participations
            .Select(p => p.UserID)
            .Distinct()
            .ToList();

        if (!userIds.Any())
        {
            return Enumerable.Empty<UserResponse>().ToPaginationResult(searchRequest);
        }

        var users = await GetUsersByIds(userIds);
        var filteredUsers = ApplyParticipationUserFilters(users, searchRequest);

        return filteredUsers.ToPaginationResult(searchRequest);
    }

    private async Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds)
    {
        var ids = userIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return Enumerable.Empty<UserResponse>();

        return await _identityMicroserviceClient.GetUsersBulk(ids);
    }

    private static IEnumerable<UserResponse> ApplyParticipationUserFilters(
        IEnumerable<UserResponse> users,
        ParticipationSearchRequest searchRequest)
    {
        IEnumerable<UserResponse> query = users;

        if (!string.IsNullOrWhiteSpace(searchRequest.ParicipationName))
        {
            var keyword = searchRequest.ParicipationName.Trim();
            query = query.Where(u =>
                ($"{u.FirstName} {u.LastName}").Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (searchRequest.DateOfBirth.HasValue)
        {
            query = query.Where(u =>
                u.DateOfBirth.HasValue
                && DateOnly.FromDateTime(u.DateOfBirth.Value.Date) == searchRequest.DateOfBirth.Value);
        }

        return query;
    }

    public async Task<IEnumerable<DTO.Response.CourseResponseDto>> GetClubCourses(Guid clubId, ClubCourseSearchRequest searchRequest)
    {
        // todo
        throw new Exception();
    }

    public async Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID()
    {
        var userID = Guid.Parse("b36eaa51-35f1-4ac7-9ca1-7c3d50bc20c6");
        var clubs = await _unitOfWork.Clubs.GetClubsByActiveParticipantUserId(userID);
        return await MapClubsWithStats(clubs);
    }

    private async Task<IEnumerable<ClubResponseDto>> MapClubsWithStats(IEnumerable<Club> clubs)
    {
        var clubList = clubs?.ToList() ?? new List<Club>();
        if (!clubList.Any())
            return Enumerable.Empty<ClubResponseDto>();

        var clubIds = clubList.Select(c => c.ClubID).Distinct().ToList();
        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds(clubIds);
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds(clubIds);

        return clubList.Select(club =>
        {
            var response = _mapper.Map<ClubResponseDto>(club);
            response.TotalMembers = memberCounts.GetValueOrDefault(club.ClubID, 0);
            response.TotalCourses = courseCounts.GetValueOrDefault(club.ClubID, 0);
            return response;
        }).ToList();
    }
}

