using System.Text.Json;

namespace Droniverse.Academy.Application.DTO.Response;

public class LabContentResponseDTO
{
    public Guid LabID { get; set; }
    public JsonElement Environment { get; set; }
}
