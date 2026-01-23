namespace Droniverse.Community.Domain.Entities;
public class ClubCategory
{
    public Category Category { get; set; }
    public Guid CategoryID { get; set; }
    public Club Club { get; set; }
    public Guid ClubID { get; set; }
}

