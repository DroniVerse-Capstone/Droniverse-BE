using Droniverse.Community.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    // thêm vào thông tin người approved, tên
    //  thông tin của người gửi request, tên của club
    public record ClubRequestResponseDto(
        Guid ClubRequestID,
        Guid RequesterID,
        Guid ApproverID,
        Guid ClubID,
        string ClubNameVN,
        string ClubNameEN,
        string RequesterName,
        string ApproverName
        )
    {
    }
}
