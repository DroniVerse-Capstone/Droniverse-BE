using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Entities
{
    public class ClubCreationRequest
    {
        public Guid ClubCreationRequestID { get; private set; }

        // ===== Editable Business Fields =====
        public string NameVN { get; set; }
        public string NameEN { get; set; }
        public string DescriptionVN { get; set; }
        public string DescriptionEN { get; set; }
        public string ClubCode { get; set; }
        public bool IsPublic { get; set; }
        public int LimitParticipant { get; set; }
        public int LimitClubManager { get; set; }
        public string ImageUrl { get; set; }

        // ===== System Fields =====
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? RejectReason { get; private set; }
        public Guid? ClubID { get; private set; }
        public Club? Club { get; private set; }
        public Guid RequesterID { get; private set; }
        public Guid? ApproverID { get; private set; }
        public ClubCreationRequestStatus Status { get; private set; }

        // ===== Constructor =====
        public ClubCreationRequest(
            string nameVN,
            string nameEN,
            string descriptionVN,
            string descriptionEN,
            string clubCode,
            bool isPublic,
            int limitParticipant,
            int limitClubManager,
            string imageUrl,
            Guid requesterId)
        {
            ClubCreationRequestID = Guid.NewGuid();
            NameVN = nameVN;
            NameEN = nameEN;
            DescriptionVN = descriptionVN;
            DescriptionEN = descriptionEN;
            ClubCode = clubCode;
            IsPublic = isPublic;
            LimitParticipant = limitParticipant;
            LimitClubManager = limitClubManager;
            ImageUrl = imageUrl;
            RequesterID = requesterId;

            CreatedAt = DateTime.UtcNow;
            Status = ClubCreationRequestStatus.PENDING;
        }

        // ===== Domain Methods =====

        public void Approve(Guid approverId, Guid clubId)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("ClubId is invalid.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Only pending request can be approved.");

            Status = ClubCreationRequestStatus.APPROVED;
            ApproverID = approverId;
            ClubID = clubId;
            ApprovedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(Guid approverId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reject reason is required.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Only PENDING request can be rejected.");

            Status = ClubCreationRequestStatus.REJECTED;
            ApproverID = approverId;
            RejectReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(Guid requesterId)
        {
            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Only PENDING request can be canceled.");

            if (RequesterID != requesterId)
                throw new InvalidOperationException("Only requester can cancel this request.");

            Status = ClubCreationRequestStatus.CANCEL;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
