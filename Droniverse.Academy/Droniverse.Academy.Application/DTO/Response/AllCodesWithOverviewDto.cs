using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public record AllCodesWithOverviewDto(
    CodeOverviewDto Overview,
    PaginationResult<IEnumerable<CodeResponseDTO>> Codes
);
