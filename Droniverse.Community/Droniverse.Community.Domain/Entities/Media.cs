namespace Droniverse.Community.Domain.Entities;
public class Media
{
    public Guid MediaID { get; set; }
    public Guid MediaTypeID { get; set; }
    public MediaType MediaType { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    // Foreign Keys
}

