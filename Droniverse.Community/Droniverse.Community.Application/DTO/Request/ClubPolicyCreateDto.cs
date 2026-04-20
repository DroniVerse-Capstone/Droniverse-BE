using Droniverse.Community.Domain.Entities;

public record ClubPolicyCreateDto
{
    public string Title { get; set; }
    public string Content { get; set; }
    public Guid ClubId { get; set; }

}