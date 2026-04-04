using Droniverse.Community.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Domain.Entities;
public class Club
{
    public Guid ClubID { get; set; } //char(36)
    public Guid CreatedBy { get; set; } //char(36) // reference to UserID
    public ICollection<ClubCategory> ClubCategories { get; set; }
    public ICollection<ClubCourse> ClubCourses { get; set; }
    public ICollection<Participation> Participations { get; set; }
    public ICollection<Competition> Competitions { get; set; }
    public ICollection<ClubAttemptRequest> ClubRequests { get; set; }
    [Length(1, 255, ErrorMessage = "Club name must be between 1 to 255 characters !")]
    public string NameVN { get; set; } //varchar(255)
    [Length(1, 255, ErrorMessage = "Club name must be between 1 to 255 characters !")]
    public string NameEN { get; set; } //varchar(255)
    public string Description { get; set; }
    [Required]
    [Length(6, 6, ErrorMessage = "Club code must be 6 characters !")]
    public string ClubCode { get; set; }
    [Required]
    public ClubStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    [Required]
    public bool IsPublic { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Club must have at least 1 member !")]
    public int LimitParticipation { get; set; }
    [Range(1, 10, ErrorMessage = "Club just has from 1 to 10 manager")]
    public int LimitClubManagers { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? SuspendedReason { get; private set; }

    // ===== Parameterless Constructor for EF Core =====
    private Club()
    {
        ClubCategories = new List<ClubCategory>();
        ClubCourses = new List<ClubCourse>();
        Participations = new List<Participation>();
        Competitions = new List<Competition>();
        ClubRequests = new List<ClubAttemptRequest>();
    }

    // ===== Constructor =====
    public Club(
        string nameVN,
        string nameEN,
        string description,
        string clubCode,
        bool isPublic,
        int limitParticipation,
        int limitClubManagers,
        Guid createdBy,
        DateTime now,
        string? imageUrl)
    {
        ClubID = Guid.NewGuid();
        NameVN = nameVN;
        NameEN = nameEN;
        Description = description;
        ClubCode = clubCode;
        IsPublic = isPublic;
        ImageUrl = imageUrl;
        LimitParticipation = limitParticipation;
        LimitClubManagers = limitClubManagers;
        CreatedBy = createdBy;

        CreatedAt = now;
        Status = ClubStatus.ACTIVE;

        ClubCategories = new List<ClubCategory>();
        ClubCourses = new List<ClubCourse>();
        Participations = new List<Participation>();
        Competitions = new List<Competition>();
        ClubRequests = new List<ClubAttemptRequest>();
    }

    // ===== Domain Methods =====

    public void Activate(DateTime now)
    {
        if (Status == ClubStatus.ACTIVE)
            throw new InvalidOperationException("Club is already active.");

        if (Status == ClubStatus.ARCHIVED)
            throw new InvalidOperationException("Archived club cannot be activated.");

        Status = ClubStatus.ACTIVE;
        UpdatedAt = now;
    }

    public void Deactivate(DateTime now)
    {
        if (Status == ClubStatus.INACTIVE)
            throw new InvalidOperationException("Club is already inactive.");

        if (Status == ClubStatus.ARCHIVED)
            throw new InvalidOperationException("Archived club cannot be deactivated.");

        Status = ClubStatus.INACTIVE;
        UpdatedAt = now;
    }

    public void Suspend(DateTime now, string reason)
    {
        if (Status == ClubStatus.SUSPENDED)
            throw new InvalidOperationException("Club is already suspended.");

        if (Status == ClubStatus.ARCHIVED)
            throw new InvalidOperationException("Archived club cannot be suspended.");

        Status = ClubStatus.SUSPENDED;
        SuspendedReason = reason;
        UpdatedAt = now;
    }

    public void Archive(DateTime now)
    {
        if (Status == ClubStatus.ARCHIVED)
            throw new InvalidOperationException("Club is already archived.");

        Status = ClubStatus.ARCHIVED;
        UpdatedAt = now;
    }

    public void Restore(DateTime now)
    {
        if (Status == ClubStatus.ARCHIVED)
            throw new InvalidOperationException("Archived club cannot be restored.");

        if (Status == ClubStatus.ACTIVE)
            throw new InvalidOperationException("Club is already active.");

        Status = ClubStatus.ACTIVE;
        UpdatedAt = now;
    }

    public bool IsActive() => Status == ClubStatus.ACTIVE;
    public bool IsSuspended() => Status == ClubStatus.SUSPENDED;
    public bool IsArchived() => Status == ClubStatus.ARCHIVED;
}
