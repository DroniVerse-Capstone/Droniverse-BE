namespace Droniverse.Community.Application.DTO.Response
{
    public record MyRoundsResultResponse
    {
        public required SimpleRoundResponse RoundInfo { get; set; }
        public required SimpleUserRoundResponse UserRoundResult { get; set; }
    }
}
