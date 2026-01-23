namespace Droniverse.Community.Domain.Entities;
public class UserCompetion
{
    
    public Guid UserID { get; set; }
    public Competition Competition { get; set; }
    public Guid CompetitionID { get; set; }
}

