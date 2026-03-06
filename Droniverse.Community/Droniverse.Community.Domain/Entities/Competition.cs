using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Competition
{
    public Guid CompetitionID { get; private set; }

    public Guid ClubID { get; private set; }
    public Club Club { get; private set; }

    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public string NameVN { get; private set; }
    public string NameEN { get; private set; }

    public string DescriptionVN { get; private set; }
    public string DescriptionEN { get; private set; }

    public string RuleContent { get; private set; }

    public int? MaxParticipants { get; private set; }

    public DateTime RegistrationStartDate { get; private set; }
    public DateTime RegistrationEndDate { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public CompetitionStatus Status { get; private set; }

    public DateTime? ResultPublishedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<CompetitionCertificate> CompetitionCertificates { get; private set; }
    public ICollection<UserCompetition> UserCompetitions { get; private set; }
    public ICollection<Round> Rounds { get; private set; }
    public ICollection<CompetitionPrize> CompetitionPrizes { get; private set; }

    private Competition() { }

    public Competition(
        Guid clubID,
        string nameVN,
        string nameEN,
        string ruleContent,
        DateTime registrationStartDate,
        DateTime registrationEndDate,
        DateTime startDate,
        DateTime endDate,
        Guid createdBy,
        int? maxParticipants = null,
        string descriptionVN = null,
        string descriptionEN = null
    )
    {
        CompetitionID = Guid.NewGuid();

        ClubID = clubID;

        NameVN = nameVN;
        NameEN = nameEN;

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        RuleContent = ruleContent;

        RegistrationStartDate = registrationStartDate;
        RegistrationEndDate = registrationEndDate;

        StartDate = startDate;
        EndDate = endDate;

        MaxParticipants = maxParticipants;

        CreatedBy = createdBy;

        Status = CompetitionStatus.DRAFT;

        CreatedAt = DateTime.UtcNow;

        CompetitionCertificates = new List<CompetitionCertificate>();
        UserCompetitions = new List<UserCompetition>();
        Rounds = new List<Round>();
        CompetitionPrizes = new List<CompetitionPrize>();
    }

    public void OpenRegistration()
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Competition must be in draft to open registration.");

        Status = CompetitionStatus.OPEN;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CloseRegistration()
    {
        if (Status != CompetitionStatus.OPEN)
            throw new InvalidOperationException("Competition is not open.");

        Status = CompetitionStatus.CLOSED;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartCompetition()
    {
        if (Status != CompetitionStatus.CLOSED)
            throw new InvalidOperationException("Competition must be closed before starting.");

        Status = CompetitionStatus.ONGOING;
        UpdatedAt = DateTime.UtcNow;
    }

    public void FinishCompetition()
    {
        if (Status != CompetitionStatus.ONGOING)
            throw new InvalidOperationException("Competition must be ongoing to finish.");

        Status = CompetitionStatus.FINISHED;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelCompetition()
    {
        if (Status == CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Finished competition cannot be cancelled.");

        Status = CompetitionStatus.CANCELLED;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PublishResult()
    {
        if (Status != CompetitionStatus.FINISHED)
            throw new InvalidOperationException("Competition must be finished before publishing results.");

        ResultPublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBasicInfo(
    string nameVN,
    string nameEN,
    Guid updatedBy)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Only draft competition can update basic information.");

        NameVN = nameVN;
        NameEN = nameEN;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(
    string descriptionVN,
    string descriptionEN,
    Guid updatedBy)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Only draft competition can update description.");

        DescriptionVN = descriptionVN;
        DescriptionEN = descriptionEN;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRule(
    string ruleContent,
    Guid updatedBy)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Cannot change rule after competition is opened.");

        RuleContent = ruleContent;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRegistrationPeriod(
    DateTime registrationStartDate,
    DateTime registrationEndDate,
    Guid updatedBy)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Cannot change registration time after opening.");

        if (registrationStartDate >= registrationEndDate)
            throw new ArgumentException("Registration start must be before registration end.");

        RegistrationStartDate = registrationStartDate;
        RegistrationEndDate = registrationEndDate;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCompetitionPeriod(
    DateTime startDate,
    DateTime endDate,
    Guid updatedBy)
    {
        if (Status == CompetitionStatus.FINISHED || Status == CompetitionStatus.CANCELLED)
            throw new InvalidOperationException("Cannot change time of finished or cancelled competition.");

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date.");

        StartDate = startDate;
        EndDate = endDate;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMaxParticipants(
    int? maxParticipants,
    Guid updatedBy)
    {
        if (Status != CompetitionStatus.DRAFT)
            throw new InvalidOperationException("Cannot change participant limit after registration opened.");

        if (maxParticipants <= 0)
            throw new ArgumentException("Max participants must be greater than 0.");

        MaxParticipants = maxParticipants;

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}