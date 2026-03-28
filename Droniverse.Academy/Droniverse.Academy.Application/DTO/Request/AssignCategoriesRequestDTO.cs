namespace Droniverse.Academy.Application.DTO.Request;

public class AssignCategoriesRequestDTO
{
    public ICollection<Guid> CategoryIDs { get; set; } = [];
}
