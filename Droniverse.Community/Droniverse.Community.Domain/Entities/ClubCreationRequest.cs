using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
        public int LimitParticipant { get; set; }
        public int LimitClubManager { get; set; }
        public string? ImageUrl { get; set; }
        public Guid MediaID { get; set; }
        public Media? Media { get; set; }
        public string? ClubPolicyVN { get; set; }
        public string? ClubPolicyEN { get; set; }
        public string? ClubRequirement { get; set; }

        // ===== System Fields =====
        public Guid DroneID { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? RejectReason { get; private set; }
        public Guid? ClubID { get; private set; }
        public Club? Club { get; private set; }
        public Guid RequesterID { get; private set; }
        public Guid? ApproverID { get; private set; }
        public ClubCreationRequestStatus Status { get; private set; }

        private ClubCreationRequest()
        {

        }

        // ===== Constructor =====
        public ClubCreationRequest(
            string nameVN,
            string nameEN,
            string description,
            int limitParticipant,
            int limitClubManager,
            string imageUrl,
            Guid requesterId,
            Guid droneID,
            Guid mediaID,
            string clubPolicyVN,
            string? clubPolicyEN,
            string? clubRequirement,
            DateTime createdAt)
        {
            ClubCreationRequestID = Guid.NewGuid();
            NameVN = nameVN;
            NameEN = nameEN;
            Description = description;
            LimitParticipant = limitParticipant;
            LimitClubManager = limitClubManager;
            ImageUrl = imageUrl;
            RequesterID = requesterId;

            CreatedAt = createdAt;
            Status = ClubCreationRequestStatus.PENDING;
            DroneID = droneID;
            ClubPolicyVN = clubPolicyVN;
            MediaID = mediaID;
            ClubPolicyEN = clubPolicyEN;
            ClubRequirement = clubRequirement;
        }

        // ===== Domain Methods =====

        public void Approve(Guid approverId, Guid clubId, DateTime now)
        {
            if (clubId == Guid.Empty)
                throw new ArgumentException("ClubId không hợp lệ.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể được phê duyệt.");

            Status = ClubCreationRequestStatus.APPROVED;
            ApproverID = approverId;
            ClubID = clubId;
            ApprovedAt = now;
            UpdatedAt = now;
        }

        public void Reject(Guid approverId, string reason, DateTime updatedAt)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do từ chối là bắt buộc.");

            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể bị từ chối.");

            Status = ClubCreationRequestStatus.REJECTED;
            ApproverID = approverId;
            RejectReason = reason;
            UpdatedAt = updatedAt;
        }

        public void Cancel(Guid requesterId, DateTime updatedAt)
        {
            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể bị hủy.");

            if (RequesterID != requesterId)
                throw new InvalidOperationException("Chỉ người tạo yêu cầu mới có thể hủy yêu cầu này.");

            Status = ClubCreationRequestStatus.CANCEL;
            UpdatedAt = updatedAt;
        }

        public void UpdateInfo(
            string nameVN,
            string nameEN,
            string description,
            int limitParticipant,
            int limitClubManager,
            string imageUrl,
            Guid requesterId,
            Guid droneID,
            string clubPolicyVN,
            string clubPolicyEN,
            Guid mediaID,
            string? clubRequirement,
            DateTime updatedAt)
        {
            if (Status != ClubCreationRequestStatus.PENDING)
                throw new InvalidOperationException("Chỉ yêu cầu ở trạng thái PENDING mới có thể được cập nhật.");

            if (RequesterID != requesterId)
                throw new InvalidOperationException("Chỉ người tạo yêu cầu mới có thể cập nhật yêu cầu này.");

            NameVN = nameVN;
            NameEN = nameEN;
            Description = description;
            LimitParticipant = limitParticipant;
            LimitClubManager = limitClubManager;
            ImageUrl = imageUrl;
            UpdatedAt = updatedAt;

            DroneID = droneID;
            ClubPolicyVN = clubPolicyVN;
            ClubPolicyEN = clubPolicyEN;
            MediaID = mediaID;
            ClubRequirement = clubRequirement;
        }
    }
}