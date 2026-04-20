using Droniverse.Community.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Domain.Entities;

public class ClubPolicy
{
    public Guid ClubPolicyID { get; private set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public Club Club { get; set; }
    public Guid ClubID { get; set; }
    public ClubCreationRequest ClubCreationRequest { get; set; }

    //CONSTRUCTOR
    public ClubPolicy(
        string Title,
        string Content,
        Guid CreatedBy,
        Guid UpdatedBy,
        Guid ClubID)
    {
        this.Title = Title;
        this.Content = Content;
        CreatedAt = DateTime.UtcNow.AddHours(7);
        UpdatedAt = DateTime.UtcNow.AddHours(7);
        this.CreatedBy = CreatedBy;
        this.UpdatedBy = UpdatedBy;
        this.ClubID = ClubID;
    }
}