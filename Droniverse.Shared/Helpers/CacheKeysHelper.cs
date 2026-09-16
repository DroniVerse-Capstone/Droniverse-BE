using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.Helpers
{
    public class CacheKeysHelper
    {
        public static string Category(Guid id) => $"category:{id}";
        public static string Course(Guid id) => $"course:{id}";
        public static string CourseVersion(Guid id) => $"course-version:{id}";
        public static string Module(Guid id) => $"module:{id}";
        public static string Lesson(Guid id) => $"lesson:{id}";
        public static string Quiz(Guid id) => $"quiz:{id}";
        public static string User(Guid id) => $"user:{id}";
        public static string UserEnrollment(Guid userId, Guid courseVersionId) => $"user-enrollment:{userId}:{courseVersionId}";
        public static string ProductReference(Guid referenceId) => $"product:reference:{referenceId}";
        public static string SystemEstimationTime() => $"system:estimation-time";

    }
}
