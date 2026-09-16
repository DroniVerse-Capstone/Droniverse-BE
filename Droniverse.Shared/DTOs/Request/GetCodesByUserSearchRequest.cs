namespace Droniverse.Shared.DTOs.Request;

public class GetCodesByUserSearchRequest : SearchRequest
{
    public bool? IsUsed { get; set; }
}
