using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Request
{
    public class CreateCourseRequest
    {   
        public Guid LevelID { get; set; }
        public CreateCourseVersionRequestDTO Version { get; set; }

    }
}
