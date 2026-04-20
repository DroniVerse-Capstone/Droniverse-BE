using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.IService;

public interface IPrerequisiteCourseService
{
    /// <summary>
    /// Replace existing prerequisites for a course with given list. Returns number of inserted records.
    /// Operation should run in a transaction inside implementation.
    /// </summary>
    Task<int> ReplacePrerequisitesAsync(Guid courseId, IEnumerable<Guid> prerequisiteCourseIds, CancellationToken cancellationToken = default);
}
