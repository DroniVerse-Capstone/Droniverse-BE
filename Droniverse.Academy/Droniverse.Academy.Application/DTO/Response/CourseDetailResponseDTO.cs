using Droniverse.Academy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Response
{
    public record CourseDetailResponseDTO
    (
        Guid CourseID,
        Guid CreateBy,
        DateTime CreateAt,
        CourseStatus Status,
        ICollection<CourseVersionResponseDTO> courseVersions
    );
}
