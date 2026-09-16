namespace Droniverse.Shared.DTOs.Request
{
    public record GetUserLevelIdsRequest
    {
        public required IEnumerable<Guid> levelIds { get; set; }
    }
}
