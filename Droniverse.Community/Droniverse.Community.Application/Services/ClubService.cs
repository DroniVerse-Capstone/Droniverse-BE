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
        if(clubRequestDto == null)
        {
            throw new ArgumentNullException(nameof(clubRequestDto), "Club request data cannot be null.");
        }

        // Validate categories exist
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

        //Gọi httpclient đến identity microservice lấy thông tin user
        UserResponse user = null;
        try
        {
             user = await _identityMicroserviceClient.GetUserByUserID(clubRequestDto.CreatedBy);
        }
        catch (Exception ex) { 
            Console.WriteLine(ex.Message );
        }

        if(user == null)
        {
            throw new KeyNotFoundException($"User with ID {clubRequestDto.CreatedBy} not found.");
        }
        club.CreateBy = user.UserId;

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

        var createdClub = await _unitOfWork.Clubs.GetByCondition(
            c => c.ClubID == club.ClubID,
            c => c.Include(i => i.ClubCategories)
        );

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(createdClub);
        //gán user vào response
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
        IEnumerable<Club> clubList = await _unitOfWork.Clubs.GetAll();
        IEnumerable<ClubResponseDto> responses = _mapper.Map<IEnumerable<ClubResponseDto>>(clubList);
        return responses;
    }

    public async Task<ClubResponseDto> GetClubById(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        return response;
    }

    public async Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto clubUpdateDto)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if(club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        _mapper.Map(clubUpdateDto, club);
        await _unitOfWork.Clubs.Update(club);
        await _unitOfWork.SaveChangeAsync();
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        return response;
    }

    public async Task<ClubResponseDto> JoinClub(ClubJoinDto request)
    {

        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubCode == request.clubCode);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with club code {request.clubCode} not found.");
        }
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        // tạo participation
        Participation newParticipation = new Participation
        {
            ParticipationID = Guid.NewGuid(),
            ClubID = club.ClubID,
            JoinDate = DateTime.Now,
            Status = club.IsPublic ? Domain.Enums.ParticipationStatus.ACTIVE : Domain.Enums.ParticipationStatus.INACTIVE,
            // lấy ID của người gửi
            UserID = Guid.Parse("ae6da7f5-1473-456f-9e55-70df702d47ee"),
            ApproverID = Guid.Empty
        };
        return response;
    }

    public async Task<PaginationResult<UserResponse>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest)
    {
        // todo
        throw new Exception();
    }

    public async Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID()
    {
        var userID = Guid.Parse("3197734d-d25d-42b1-b968-84b6ee4d33c2");
        IEnumerable<Participation> participations = await _unitOfWork.Participations.GetManyByCondition(p => p.UserID == userID  && p.Status == Domain.Enums.ParticipationStatus.ACTIVE);
        var clubs = participations.Select(p => p.Club);
        return _mapper.Map<IEnumerable<ClubResponseDto>>(clubs);
    }
}

