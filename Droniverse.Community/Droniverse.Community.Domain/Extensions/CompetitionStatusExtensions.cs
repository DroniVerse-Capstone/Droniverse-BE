using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Extensions
{
    public static class CompetitionStatusExtensions
    {
        public static bool IsVisible(this CompetitionStatus status)
            => status != CompetitionStatus.DRAFT;

        public static bool CanRegister(this CompetitionStatus status)
            => status == CompetitionStatus.PUBLISHED;

        public static bool IsFinished(this CompetitionStatus status)
            => status == CompetitionStatus.RESULT_PUBLISHED
            || status == CompetitionStatus.CANCELLED
            || status == CompetitionStatus.INVALID;
    }
}
