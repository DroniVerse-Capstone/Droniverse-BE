using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;

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
        Club club = _mapper.Map<Club>(clubRequestDto);
        club.ClubID = Guid.NewGuid();
        club.ClubCode = Guid.NewGuid();

        // Academy microservice
        //FeedbackResponseDto feedback = await _academyMicroserviceClient.GetFeedbackById(clubRequestDto.FeedbackId);
        //if(feedback == null)
        //{
        //    throw new KeyNotFoundException($"Feedback with ID {clubRequestDto.FeedbackId} not found.");
        //}

        //Gọi httpclient đến identity microservice lấy thông tin user
        UserResponse user = await _identityMicroserviceClient.GetUserByUserID(clubRequestDto.CreatedBy);
        if(user == null)
        {
            throw new KeyNotFoundException($"User with ID {clubRequestDto.CreatedBy} not found.");
        }
        club.CreateBy = user.UserId;

        await _unitOfWork.Clubs.Add(club);
        await _unitOfWork.SaveChangeAsync();
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        //gán user vào response
        response = response with { Creator = user };
        return response;
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
}

