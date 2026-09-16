using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.DTO.Response;

public record CodeOverviewDto(
    int TotalCodes,
    int AvailableCodes,
    int UsedCodes,
    int ExpiredCodes
)
{
    public CodeOverviewDto() : this(0, 0, 0, 0)
    {
    }
}
