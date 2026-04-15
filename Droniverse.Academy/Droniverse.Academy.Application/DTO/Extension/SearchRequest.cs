using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Extension
{
    public class UserEnrollmentSearchRequest : SearchRequest
    {
        public CourseLevel? Level { get; set; }
        public string? CourseSearchName { get; set; }
        public UserEnrollment? EnrollmentStatus { get; set; }
    }

    public enum UserEnrollment
    {
        ACTIVE = 0,
        COMPLETED = 1
    }

    public class GetUsersNoCodesSearchRequest : SearchRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public UserCodesEnum? UserCodes { get; set; } = UserCodesEnum.User_Has_Codes;
    }

    public enum UserCodesEnum
    {
        User_No_Codes,
        User_Has_Codes
    }
}
