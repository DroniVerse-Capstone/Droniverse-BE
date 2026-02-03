using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Domain.Enums
{
    public enum CourseVersionStatus
    {
        DRAFT = 0,
        ACTIVE = 1,      // cho học và enroll
        DEPRECATED = 2,  // chỉ cho học không cho enroll
        INACTIVE = 3
    }
}
