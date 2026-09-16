namespace Droniverse.Community.Domain.Entities;
public class Media
{
    public Guid MediaID { get; set; }
    public Guid MediaTypeID { get; set; }
    public MediaType MediaType { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ClubCreationRequest> ClubCreationRequests { get; set; } = new List<ClubCreationRequest>();
    public ICollection<ClubAttemptRequest> ClubAttemptRequests { get; set; } = new List<ClubAttemptRequest>();
    // Foreign Keys
}

