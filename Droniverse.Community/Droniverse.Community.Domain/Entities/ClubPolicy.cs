using Droniverse.Community.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Domain.Entities;

public class ClubPolicy
{
    public Guid ClubPolicyID { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public Club Club { get; set; }
    public Guid ClubID { get; set; }
    public ClubCreationRequest ClubCreationRequest { get; set; }
}
