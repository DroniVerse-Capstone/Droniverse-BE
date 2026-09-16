namespace Droniverse.Shared.DTOs
{
    public record SimpleUserReponse
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; } = null;
    }
}
