using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionCreationRequest
    {
        [Required]
        [Length(1,255, ErrorMessage = "Tên tiếng Việt của cuộc thi phải từ 1 đến 255 kí tự !")]
        public required string NameVN { get; set; }
        [Required]
        [Length(1, 255, ErrorMessage = "Tên tiếng Anh của cuộc thi phải từ 1 đến 255 kí tự !")]
        public required string NameEN { get; set; }
        [Required]
        [Length(1, 255, ErrorMessage = "Mô tả tiếng Việt của cuộc thi phải từ 1 đến 255 kí tự !")]
        public string? DescriptionVN { get; set; }

        [Length(1, 255, ErrorMessage = "Mô tả tiếng Anh của cuộc thi phải từ 1 đến 255 kí tự !")]
        public string? DescriptionEN { get; set; }
        [Required]
        public required string RuleContent { get; set; }
        public int? MaxParticipants { get; set; } // nếu null thì là không giới hạn người tham gia
        [Required]
        public DateTime VisibleAt { get; set; }
        [Required]
        public DateTime RegistrationStartDate { get; set; }
        [Required]
        public DateTime RegistrationEndDate { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime? ResultPublishedAt { get; set; }
        [Required]
        public Guid ClubID { get; set; }
    }
}
