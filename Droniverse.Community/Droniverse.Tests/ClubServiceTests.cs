using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.Services;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Services.IServices;
using AutoMapper;
using Moq;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;
using Droniverse.Shared.DTOs.Request;

namespace Droniverse.Tests;

public class ClubServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IdentityMicroserviceClient> _identityClientMock;
    private readonly Mock<AcademyMicroserviceClient> _academyClientMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IClock> _clockMock;
    private readonly ClubService _clubService;

    public ClubServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        // Use default HttpClients to satisfy constructor requirements
        // A more advanced mock would use HttpMessageHandler
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");

        _identityClientMock = new Mock<IdentityMicroserviceClient>(httpClient, null, null, null);
        _academyClientMock = new Mock<AcademyMicroserviceClient>(httpClient, null, null, null, null, _identityClientMock.Object, null);

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _clockMock = new Mock<IClock>();
        _clockMock.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

        _clubService = new ClubService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _identityClientMock.Object,
            _academyClientMock.Object,
            _currentUserServiceMock.Object,
            _clockMock.Object
        );
    }

    // ===== Helper Methods =====
    private Club CreateTestClub(Guid? clubId = null, string? clubCode = null, Guid? createdBy = null)
    {
        var id = clubId ?? Guid.NewGuid();
        var code = clubCode ?? "123456";
        return new Club(
            nameVN: "Test Club VN",
            nameEN: "Test Club EN",
            description: "Test Description",
            clubCode: code,
            limitParticipation: 10,
            limitClubManagers: 2,
            createdBy: createdBy ?? Guid.NewGuid(),
            now: _clockMock.Object.Now,
            imageUrl: null,
            managerID: Guid.NewGuid(),
            droneID: Guid.NewGuid(),
            clubPolicyVN: "Policy VN",
            clubPolicyEN: "Policy EN"
        );
    }

    private static ClubResponseDto CreateTestClubResponseDto(Guid? clubId = null)
    {
        return new ClubResponseDto
        {
            ClubID = clubId ?? Guid.NewGuid(),
            NameVN = "Test Club VN",
            NameEN = "Test Club EN",
            DescriptionVN = "Test Description VN",
            DescriptionEN = "Test Description EN",
            ClubCode = "123456"
        };
    }

    // ===== Tests =====
    [Fact]
    public async Task CreateClub_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _clubService.CreateClub(null!));
    }

    [Fact]
    public async Task DeleteClub_ShouldThrowKeyNotFoundException_WhenClubDoesNotExist()
    {
        // Arrange
        var clubId = Guid.NewGuid();
        // Setup GetByCondition returning null
        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync((Club?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.DeleteClub(clubId));
    }

    [Fact]
    public async Task GetClubById_ShouldThrowKeyNotFoundException_WhenClubDoesNotExist()
    {
        var clubId = Guid.NewGuid();
        _unitOfWorkMock.Setup(u => u.Clubs.GetByIdWithCategories(clubId))
            .ReturnsAsync((Club?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.GetClubById(clubId));
    }

    [Fact]
    public async Task CheckParticipant_ShouldThrowArgumentException_WhenClubIdIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _clubService.CheckParticipant(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public async Task CheckParticipant_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _clubService.CheckParticipant(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public async Task GetClubByClubCode_ShouldThrowKeyNotFoundException_WhenClubNotFound()
    {
        _unitOfWorkMock.Setup(u => u.Clubs.GetByClubCodeWithCategories("123456"))
            .ReturnsAsync((Club?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.GetClubByClubCode("123456"));
    }

    [Fact]
    public async Task UpdateClub_ShouldThrowKeyNotFoundException_WhenClubNotFound()
    {
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync((Club?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.UpdateClub(id, new ClubUpdateDto()));
    }

    [Fact]
    public async Task JoinClub_ShouldThrowKeyNotFoundException_WhenClubNotFound()
    {
        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync((Club?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.JoinClub(new ClubJoinDto("123456", null, "rule")));
    }

    [Fact]
    public async Task GetClubInfoBulk_ShouldReturnEmpty_WhenIdsEmpty()
    {
        var result = await _clubService.GetClubInfoBulk(new GetClubSimpleInfoRequest { ClubIds = new List<Guid>() });
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDroneFromClub_ShouldReturnEmptyGuid_WhenClubNotFound()
    {
        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync((Club?)null);
        var result = await _clubService.GetDroneFromClub(Guid.NewGuid());
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public async Task UpdateClubStatus_ShouldThrowKeyNotFoundException_WhenClubNotFound()
    {
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(u => u.Clubs.GetByIdWithCategories(id))
            .ReturnsAsync((Club?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.UpdateClubStatus(id, new ClubUpdateStatusDto { Status = ClubStatus.ACTIVE }));
    }

    [Fact]
    public async Task GetClubParticipantIds_ShouldThrowKeyNotFoundException_WhenClubNotFound()
    {
        var id = Guid.NewGuid();
        _unitOfWorkMock.Setup(u => u.Clubs.GetSimpleClubInfoById(id))
            .ReturnsAsync((SimpleClubResponse?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _clubService.GetClubParticipantIds(id, new GetClubParticipantIdsRequest()));
    }

    [Fact]
    public async Task CreateClub_ShouldReturnClubResponseDto_WhenValidRequest()
    {
        var clubRequestDto = new ClubCreateDto();
        var clubId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var club = CreateTestClub(clubId);
        var clubResponseDto = CreateTestClubResponseDto(clubId);

        _mapperMock.Setup(m => m.Map<Club>(clubRequestDto)).Returns(club);
        _currentUserServiceMock.Setup(s => s.UserID).Returns(currentUserId.ToString());
        _unitOfWorkMock.Setup(u => u.Clubs.Add(It.IsAny<Club>())).ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.Clubs.GetClubStatsByClubIds(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new Dictionary<Guid, (int MemberCount, int CourseCount)>());
        _mapperMock.Setup(m => m.Map<ClubResponseDto>(It.IsAny<Club>())).Returns(clubResponseDto);

        var result = await _clubService.CreateClub(clubRequestDto);

        Assert.NotNull(result);
        _unitOfWorkMock.Verify(u => u.Clubs.Add(It.IsAny<Club>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClub_ShouldReturnTrue_WhenClubIsDeletedSuccessfully()
    {
        var clubId = Guid.NewGuid();
        var club = CreateTestClub(clubId);

        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.Clubs.Delete(club)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

        var result = await _clubService.DeleteClub(clubId);

        Assert.True(result);
        _unitOfWorkMock.Verify(u => u.Clubs.Delete(club), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllClubs_ShouldReturnEmptyPaginationResult_WhenNoClubsFound()
    {
        var request = new GetAllClubsSearchRequest { CurrentPage = 1, PageSize = 10 };
        var pagedResult = new PaginationResult<IEnumerable<Club>>([], 0, 1, 10);

        _unitOfWorkMock.Setup(u => u.Clubs.GetAllWithPolicies(request.ClubName, request.ClubStatus, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(pagedResult);

        var result = await _clubService.GetAllClubs(request);

        Assert.NotNull(result);
        Assert.Empty(result.Data!);
        Assert.Equal(0, result.TotalRecords);
    }

    [Fact]
    public async Task GetClubById_ShouldReturnClubResponse_WhenClubExists()
    {
        var clubId = Guid.NewGuid();
        var club = CreateTestClub(clubId);
        var clubResponseDto = CreateTestClubResponseDto(clubId);

        _unitOfWorkMock.Setup(u => u.Clubs.GetByIdWithCategories(clubId)).ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.Clubs.GetClubStatsByClubIds(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, (int MemberCount, int CourseCount)>());
        _mapperMock.Setup(m => m.Map<ClubResponseDto>(club)).Returns(clubResponseDto);

        var result = await _clubService.GetClubById(clubId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateClub_ShouldReturnUpdatedClub_WhenValidRequest()
    {
        var id = Guid.NewGuid();
        var clubUpdateDto = new ClubUpdateDto();
        var club = CreateTestClub(id);
        var updatedClubResponseDto = CreateTestClubResponseDto(id);

        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.Clubs.Update(club)).ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Clubs.GetByIdWithCategories(id)).ReturnsAsync(club);

        _unitOfWorkMock.Setup(u => u.Clubs.GetClubStatsByClubIds(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, (int MemberCount, int CourseCount)>());
        _mapperMock.Setup(m => m.Map<ClubResponseDto>(club)).Returns(updatedClubResponseDto);

        var result = await _clubService.UpdateClub(id, clubUpdateDto);

        Assert.NotNull(result);
        _unitOfWorkMock.Verify(u => u.Clubs.Update(club), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangeAsync(), Times.Once);
    }

    [Fact]
    public async Task JoinClub_ShouldReturnResponse_WhenValidRequest()
    {
        var clubCode = "123456";
        var club = CreateTestClub(clubCode: clubCode);
        var currentUserId = Guid.NewGuid();
        var request = new ClubJoinDto(clubCode, null, "Requirement");

        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync(club);
        _currentUserServiceMock.Setup(s => s.UserID).Returns(currentUserId.ToString());
        _unitOfWorkMock.Setup(u => u.Participations.IsUserInClub(club.ClubID, currentUserId)).ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.ClubAttemptRequests.IsUserInClubAttemptRequest(currentUserId, club.ClubID)).ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.ClubAttemptRequests.Add(It.IsAny<ClubAttemptRequest>())).Returns<ClubAttemptRequest>(r => Task.FromResult(r));
        _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

        var result = await _clubService.JoinClub(request);

        Assert.NotNull(result);
        Assert.Equal(club.ClubID, result.ClubID);
        _unitOfWorkMock.Verify(u => u.ClubAttemptRequests.Add(It.IsAny<ClubAttemptRequest>()), Times.Once);
    }

    [Fact]
    public async Task LeaveClub_ShouldMarkParticipationLeft_WhenMemberIsActive()
    {
        var clubId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var participation = new Participation(currentUserId, clubId, null, _clockMock.Object.Now);

        _currentUserServiceMock.Setup(s => s.UserID).Returns(currentUserId.ToString());
        _unitOfWorkMock.Setup(u => u.Participations.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Participation, bool>>>(), It.IsAny<Func<IQueryable<Participation>, IQueryable<Participation>>>() ))
            .ReturnsAsync(participation);
        _unitOfWorkMock.Setup(u => u.Participations.Update(participation)).ReturnsAsync(participation);
        _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

        var result = await _clubService.LeaveClub(clubId);

        Assert.True(result);
        Assert.Equal(ParticipationStatus.LEFT, participation.Status);
        Assert.NotNull(participation.LeftDate);
    }

    [Fact]
    public async Task KickMemberFromClub_ShouldMarkParticipationBanned_WhenAllowedUserKicksMember()
    {
        var clubId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var club = CreateTestClub(clubId, createdBy: currentUserId);
        var participation = new Participation(targetUserId, clubId, null, _clockMock.Object.Now);
        var request = new ClubKickMemberRequest();

        _currentUserServiceMock.Setup(s => s.UserID).Returns(currentUserId.ToString());
        _currentUserServiceMock.Setup(s => s.Roles).Returns(new List<string> { Droniverse.Shared.Constants.Roles.ClubManager });
        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>() ))
            .ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.Participations.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Participation, bool>>>(), It.IsAny<Func<IQueryable<Participation>, IQueryable<Participation>>>() ))
            .ReturnsAsync(participation);
        _unitOfWorkMock.Setup(u => u.Participations.Update(participation)).ReturnsAsync(participation);
        _unitOfWorkMock.Setup(u => u.SaveChangeAsync()).ReturnsAsync(1);

        var result = await _clubService.KickMemberFromClub(clubId, targetUserId, request);

        Assert.NotNull(result);
        Assert.Equal(club.NameVN, result.ClubName);
        Assert.Equal("Thành viên", result.Username);
        Assert.Equal(ParticipationStatus.BANNED, participation.Status);
        Assert.Equal("Vi phạm nội quy", participation.Note);
        Assert.NotNull(participation.LeftDate);
    }

    [Fact]
    public async Task GetClubParcitipations_ShouldReturnPaginationResult()
    {
        var clubId = Guid.NewGuid();
        var club = CreateTestClub(clubId);
        var searchRequest = new ParticipationSearchRequest { CurrentPage = 1, PageSize = 10 };

        _unitOfWorkMock.Setup(u => u.Clubs.GetByCondition(It.IsAny<System.Linq.Expressions.Expression<Func<Club, bool>>>(), It.IsAny<Func<IQueryable<Club>, IQueryable<Club>>>()))
            .ReturnsAsync(club);

        var participations = new List<Participation>();
        _unitOfWorkMock.Setup(u => u.Participations.GetActiveParticipationsByClubAsync(clubId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<Guid>?>()))
            .ReturnsAsync((1, participations));

        var result = await _clubService.GetClubParcitipations(clubId, searchRequest);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetClubsByCurrentUsersID_ShouldReturnList()
    {
        var currentUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserID).Returns(currentUserId.ToString());
        _currentUserServiceMock.Setup(s => s.Roles).Returns(new List<string> { Droniverse.Shared.Constants.Roles.ClubMember });

        var clubs = new List<Club> { CreateTestClub() };
        _unitOfWorkMock.Setup(u => u.Clubs.GetClubsByParticipantUserId(currentUserId, null))
            .ReturnsAsync(clubs);

        _unitOfWorkMock.Setup(u => u.Clubs.GetClubStatsByClubIds(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, (int MemberCount, int CourseCount)>());

        var result = await _clubService.GetClubsByCurrentUsersID();

        Assert.NotNull(result);
    }
}
