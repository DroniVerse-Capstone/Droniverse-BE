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
        public string Description { get; set; }
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
        public ICollection<ClubCreationRequestCategory> Categories { get; private set; }

        private ClubCreationRequest()
        {

        }

        // ===== Constructor =====
        public ClubCreationRequest(
            string nameVN,
            string nameEN,
            string description,
            bool isPublic,
            int limitParticipant,
            int limitClubManager,
            string imageUrl,
            Guid requesterId)
        {
            ClubCreationRequestID = Guid.NewGuid();
            NameVN = nameVN;
            NameEN = nameEN;
            Description = description;
            IsPublic = isPublic;
            LimitParticipant = limitParticipant;
            LimitClubManager = limitClubManager;
            ImageUrl = imageUrl;
            RequesterID = requesterId;

            CreatedAt = DateTime.UtcNow;
            Status = ClubCreationRequestStatus.PENDING;
            Categories = new List<ClubCreationRequestCategory>();
        }

        // ===== Domain Methods =====

        public void Approve(Guid approverId, Guid clubId)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("ClubId không hợp lệ.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể được phê duyệt.");

            Status = ClubCreationRequestStatus.APPROVED;
            ApproverID = approverId;
            ClubID = clubId;
            ApprovedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(Guid approverId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do từ chối là bắt buộc.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể bị từ chối.");

            Status = ClubCreationRequestStatus.REJECTED;
            ApproverID = approverId;
            RejectReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(Guid requesterId)
        {
            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể bị hủy.");

            if (RequesterID != requesterId)
                throw new InvalidOperationException("Chỉ người tạo yêu cầu mới có thể hủy yêu cầu này.");

            Status = ClubCreationRequestStatus.CANCEL;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateInfo(
            string nameVN,
            string nameEN,
            string description,
            bool isPublic,
            int limitParticipant,
            int limitClubManager,
            string imageUrl,
            Guid requesterId)
        {
            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể được cập nhật.");

            if (RequesterID != requesterId)
                throw new InvalidOperationException("Chỉ người tạo yêu cầu mới có thể cập nhật yêu cầu này.");

            NameVN = nameVN;
            NameEN = nameEN;
            Description = description;
            IsPublic = isPublic;
            LimitParticipant = limitParticipant;
            LimitClubManager = limitClubManager;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}