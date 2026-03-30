namespace Droniverse.Academy.Application.DTO.Request;

public class AddRequiredDronesRequestDTO
{
    public ICollection<Guid> DroneIDs { get; set; } = [];
}
