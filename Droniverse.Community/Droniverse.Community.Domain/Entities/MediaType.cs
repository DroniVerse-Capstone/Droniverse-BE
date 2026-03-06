namespace Droniverse.Community.Domain.Entities;
public class MediaType
{
    public Guid MediaTypeID { get; set; }
    public ICollection<Media> Medias { get; set; }
    public string TypeNameVN { get; set; } //varchar(50)
    public string TypeNameEN { get; set; }
    public string DescriptionEN { get; set; } //text
    public string DescriptionVN { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

