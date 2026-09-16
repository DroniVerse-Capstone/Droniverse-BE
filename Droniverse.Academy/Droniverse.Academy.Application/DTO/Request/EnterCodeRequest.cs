using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Request
{
    public record EnterCodeRequest
    {
        [Required(ErrorMessage = "Cần có mã tham gia")]
        public required string CodeId { get; set; }
    }
}
