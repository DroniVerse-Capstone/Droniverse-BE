namespace Droniverse.Academy.Domain.Entities;
public class UserLab
{
    public Guid UserLabID { get; set; } //char(36)
    public Guid UserID { get; set; } //char(36)
    public Lab Lab { get; set; }
    public Guid LabID { get; set; } //char(36)
    public string Solution { get; set; }
    public bool IsCompleted { get; set; }
    public float Time { get; set; }
    public int NumberOfStep { get; set; }
    public float Length { get; set; }
    public string FeedbackVN { get; set; }
    public string FeedbackEN { get; set; }
    public decimal Point { get; set; }
}
