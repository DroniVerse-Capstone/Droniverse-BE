using Droniverse.Community.Application.Services;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.QueryModels;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Droniverse.Tests;

public class CompetitionServiceAggregateLeaderBoardAsyncTests
{
    [Fact]
    public async Task AggregateLeaderBoardAsync_ShouldThrow_WhenCompetitionNotFound()
    {
        var now = DateTime.UtcNow;
        var competitionRepoMock = new Mock<ICompetitionRepository>();
        competitionRepoMock
            .Setup(x => x.GetByCondition(
                It.IsAny<Expression<Func<Competition, bool>>>(),
                It.IsAny<Func<IQueryable<Competition>, IQueryable<Competition>>?>()))
            .ReturnsAsync((Competition?)null);

        var userRoundRepoMock = new Mock<IUserRoundRepository>();
        var userPrizeRepoMock = new Mock<IUserPrizeRepository>();
        var unitOfWorkMock = CreateUnitOfWork(competitionRepoMock, userRoundRepoMock, userPrizeRepoMock);
        var service = CreateService(unitOfWorkMock.Object, now, Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AggregateLeaderBoardAsync(Guid.NewGuid()));

        unitOfWorkMock.Verify(x => x.SaveChangeAsync(), Times.Never);
        userPrizeRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()), Times.Never);
    }

    [Fact]
    public async Task AggregateLeaderBoardAsync_ShouldThrow_WhenCompetitionIsNotPublished()
    {
        var now = DateTime.UtcNow;
        var competition = CreateDraftCompetition(now);
        var competitionRepoMock = new Mock<ICompetitionRepository>();
        competitionRepoMock
            .Setup(x => x.GetByCondition(
                It.IsAny<Expression<Func<Competition, bool>>>(),
                It.IsAny<Func<IQueryable<Competition>, IQueryable<Competition>>?>()))
            .ReturnsAsync(competition);

        var userRoundRepoMock = new Mock<IUserRoundRepository>();
        var userPrizeRepoMock = new Mock<IUserPrizeRepository>();
        var unitOfWorkMock = CreateUnitOfWork(competitionRepoMock, userRoundRepoMock, userPrizeRepoMock);
        var service = CreateService(unitOfWorkMock.Object, now, Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AggregateLeaderBoardAsync(competition.CompetitionID));

        userRoundRepoMock.Verify(x => x.GetCompetitionLeaderboardAll(It.IsAny<Guid>()), Times.Never);
        userPrizeRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangeAsync(), Times.Never);
    }

    [Fact]
    public async Task AggregateLeaderBoardAsync_ShouldThrow_WhenCompetitionNotEnded()
    {
        var now = DateTime.UtcNow;
        var competition = CreatePublishedCompetition(now, now.AddHours(2));
        var competitionRepoMock = new Mock<ICompetitionRepository>();
        competitionRepoMock
            .Setup(x => x.GetByCondition(
                It.IsAny<Expression<Func<Competition, bool>>>(),
                It.IsAny<Func<IQueryable<Competition>, IQueryable<Competition>>?>()))
            .ReturnsAsync(competition);

        var userRoundRepoMock = new Mock<IUserRoundRepository>();
        var userPrizeRepoMock = new Mock<IUserPrizeRepository>();
        var unitOfWorkMock = CreateUnitOfWork(competitionRepoMock, userRoundRepoMock, userPrizeRepoMock);
        var service = CreateService(unitOfWorkMock.Object, now, Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AggregateLeaderBoardAsync(competition.CompetitionID));

        userRoundRepoMock.Verify(x => x.GetCompetitionLeaderboardAll(It.IsAny<Guid>()), Times.Never);
        userPrizeRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangeAsync(), Times.Never);
    }

    [Fact]
    public async Task AggregateLeaderBoardAsync_ShouldReturnFalse_WhenNoLeaderboardEntries()
    {
        var now = DateTime.UtcNow;
        var competition = CreatePublishedCompetition(now, now.AddHours(-1));

        var competitionRepoMock = new Mock<ICompetitionRepository>();
        competitionRepoMock
            .Setup(x => x.GetByCondition(
                It.IsAny<Expression<Func<Competition, bool>>>(),
                It.IsAny<Func<IQueryable<Competition>, IQueryable<Competition>>?>()))
            .ReturnsAsync(competition);

        var userRoundRepoMock = new Mock<IUserRoundRepository>();
        userRoundRepoMock
            .Setup(x => x.GetCompetitionLeaderboardAll(competition.CompetitionID))
            .ReturnsAsync([]);

        var userPrizeRepoMock = new Mock<IUserPrizeRepository>();
        var unitOfWorkMock = CreateUnitOfWork(competitionRepoMock, userRoundRepoMock, userPrizeRepoMock);
        var service = CreateService(unitOfWorkMock.Object, now, Guid.NewGuid());

        var result = await service.AggregateLeaderBoardAsync(competition.CompetitionID);

        Assert.False(result);
        Assert.Equal(CompetitionStatus.PUBLISHED, competition.Status);
        userPrizeRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangeAsync(), Times.Never);
    }

    [Fact]
    public async Task AggregateLeaderBoardAsync_ShouldAggregateAndPublishResults_WhenEntriesExist()
    {
        var now = DateTime.UtcNow;
        var currentUserId = Guid.NewGuid();
        var competition = CreatePublishedCompetition(now, now.AddHours(-1));
        competition.AddPrize("Top 1", "Top 1", RewardType.MONEY, 1, 1, currentUserId, rewardValueMoney: 100, createdAt: now);
        competition.AddPrize("Top 2-3", "Top 2-3", RewardType.GIFT, 2, 3, currentUserId, rewardValueGiftVN: "Gift VN", rewardValueGiftEN: "Gift EN", createdAt: now);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();
        var user4 = Guid.NewGuid();

        var entries = new List<CompetitionLeaderboardQueryModel>
        {
            new() { UserId = user1, Score = 100, TotalTime = TimeSpan.FromMinutes(2) },
            new() { UserId = user2, Score = 80, TotalTime = TimeSpan.FromMinutes(1) },
            new() { UserId = user3, Score = 100, TotalTime = TimeSpan.FromMinutes(1) },
            new() { UserId = user4, Score = 70, TotalTime = TimeSpan.FromMinutes(5) }
        };

        var competitionRepoMock = new Mock<ICompetitionRepository>();
        competitionRepoMock
            .Setup(x => x.GetByCondition(
                It.IsAny<Expression<Func<Competition, bool>>>(),
                It.IsAny<Func<IQueryable<Competition>, IQueryable<Competition>>?>()))
            .ReturnsAsync(competition);

        var userRoundRepoMock = new Mock<IUserRoundRepository>();
        userRoundRepoMock
            .Setup(x => x.GetCompetitionLeaderboardAll(competition.CompetitionID))
            .ReturnsAsync(entries);

        IEnumerable<UserPrize>? capturedPrizes = null;
        var userPrizeRepoMock = new Mock<IUserPrizeRepository>();
        userPrizeRepoMock
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()))
            .Callback<IEnumerable<UserPrize>>(x => capturedPrizes = x.ToList())
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = CreateUnitOfWork(competitionRepoMock, userRoundRepoMock, userPrizeRepoMock);
        unitOfWorkMock.Setup(x => x.SaveChangeAsync()).ReturnsAsync(1);

        var service = CreateService(unitOfWorkMock.Object, now, currentUserId);

        var result = await service.AggregateLeaderBoardAsync(competition.CompetitionID);

        Assert.True(result);
        Assert.NotNull(capturedPrizes);
        var prizes = capturedPrizes!.ToList();
        Assert.Equal(3, prizes.Count);

        Assert.Contains(prizes, p => p.UserID == user3 && p.Rank == 1 && p.RewardType == RewardType.MONEY && p.RewardValueMoney == 100);
        Assert.Contains(prizes, p => p.UserID == user1 && p.Rank == 2 && p.RewardType == RewardType.GIFT && p.RewardValueGiftVN == "Gift VN");
        Assert.Contains(prizes, p => p.UserID == user2 && p.Rank == 3 && p.RewardType == RewardType.GIFT && p.RewardValueGiftVN == "Gift VN");
        Assert.DoesNotContain(prizes, p => p.UserID == user4);

        Assert.Equal(CompetitionStatus.RESULT_PUBLISHED, competition.Status);
        Assert.True(competition.IsSummarized);
        Assert.Equal(now, competition.ResultPublishedAt);

        userPrizeRepoMock.Verify(x => x.AddRange(It.IsAny<IEnumerable<UserPrize>>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangeAsync(), Times.Once);
    }

    private static CompetitionService CreateService(IUnitOfWork unitOfWork, DateTime now, Guid currentUserId)
    {
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.SetupGet(x => x.UserId).Returns(currentUserId);

        var clockMock = new Mock<IClock>();
        clockMock.SetupGet(x => x.Now).Returns(now);

        var distributedCacheMock = new Mock<IDistributedCache>();
        var loggerMock = new Mock<ILogger<CompetitionService>>();

        return new CompetitionService(
            unitOfWork,
            currentUserServiceMock.Object,
            clockMock.Object,
            null!,
            null!,
            distributedCacheMock.Object,
            loggerMock.Object);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<ICompetitionRepository> competitionRepository,
        Mock<IUserRoundRepository> userRoundRepository,
        Mock<IUserPrizeRepository> userPrizeRepository)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(x => x.Competitions).Returns(competitionRepository.Object);
        unitOfWorkMock.SetupGet(x => x.UserRounds).Returns(userRoundRepository.Object);
        unitOfWorkMock.SetupGet(x => x.UserPrizes).Returns(userPrizeRepository.Object);
        unitOfWorkMock.Setup(x => x.SaveChangeAsync()).ReturnsAsync(1);
        return unitOfWorkMock;
    }

    private static Competition CreateDraftCompetition(DateTime now)
    {
        return new Competition(
            Guid.NewGuid(),
            "Competition VN",
            "Competition EN",
            "Rules",
            now.AddDays(-10),
            now.AddDays(-9),
            now.AddDays(-8),
            now.AddDays(-7),
            now.AddDays(-1),
            Guid.NewGuid(),
            now.AddDays(-12));
    }

    private static Competition CreatePublishedCompetition(DateTime now, DateTime endDate)
    {
        var competition = new Competition(
            Guid.NewGuid(),
            "Competition VN",
            "Competition EN",
            "Rules",
            now.AddDays(-10),
            now.AddDays(-9),
            now.AddDays(-8),
            now.AddDays(-7),
            endDate,
            Guid.NewGuid(),
            now.AddDays(-12));

        competition.SystemPublish(now.AddDays(-6));
        return competition;
    }
}
