using Droniverse.Community.Application.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface IClubCourseService
    {
        Task<bool> AddCourseToClub(Guid clubId, Guid courseId, AddClubCourseRequest request);
    }
}
