using Droniverse.Shared.DTOs;
using System;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubMemberTransactionResponseDto
    {
        public SimpleUserReponse User { get; set; } = default!;
        public Guid CourseId { get; set; }
        public string CourseNameVN { get; set; } = string.Empty;
        public string CourseNameEN { get; set; } = string.Empty;
        public string? CourseImageUrl { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
