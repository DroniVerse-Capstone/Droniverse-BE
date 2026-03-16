using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record ClubRequestResponseDto(
    Guid ClubRequestID,
    Guid RequesterID,
    Guid? ApproverID,
    Guid ClubID,
    string ClubNameVN,
    string ClubNameEN,
    string? RequesterName,
    string? ApproverName,
    ClubAttemptRequestStatus Status,
    DateTime createAt,
    DateTime? processedAt
    );
}
