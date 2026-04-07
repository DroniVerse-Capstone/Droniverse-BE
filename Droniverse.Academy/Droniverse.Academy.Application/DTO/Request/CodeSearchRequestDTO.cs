using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository.SearchSpec;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.Application.DTO.Request;

public class CodeSearchRequestDTO : SearchRequest, ICodeSearchSpec
{
    public CodeStatus? Status { get; set; } //ACTIVE, INACTIVE
    [FromQuery(Name = "codeUsageStatus")]
    public CodeUsageStatus? CodeUsageStatus { get; set; }
}

