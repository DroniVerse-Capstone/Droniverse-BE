namespace Droniverse.Community.Domain.Entities;
public class Category // Category của khóa học
{
    public Guid CategoryID { get; set; }
    public string TypeNameVN { get; set; } //varchar(255)
    public string TypeNameEN { get; set; } //varchar(255)
    public string DescriptionVN { get; set; } //text
    public string DescriptionEN { get; set; } //text
    public ICollection<ClubCreationRequestCategory> ClubCreationRequests { get; private set; }
    public ICollection<ClubCategory> ClubCategories { get; set; }


}
