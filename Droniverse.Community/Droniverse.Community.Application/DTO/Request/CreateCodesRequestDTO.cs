

namespace Droniverse.Community.Application.DTO.Request
{
    public record CreateCodesRequestDTO
    {
        public required Guid CourseId { get; set; }
        public required int Quantity { get; set; }
    }
}
