namespace Droniverse.Community.Application.DTO.Response;

public class CourseResponseDto
{
    public Guid CourseID { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Level { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; }
}