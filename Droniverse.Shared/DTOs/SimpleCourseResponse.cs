namespace Droniverse.Shared.DTOs
{
    public record SimpleCourseResponse
    {
        public Guid CourseId { get; set; }
        public required string CourseNameVN { get; set; }
        public required string CourseNameEN { get; set; }
        public required string ImageUrl { get; set; }
    }
}
