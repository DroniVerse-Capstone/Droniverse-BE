using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public class SimplePrizeResponse
    {
        public Guid CompetitionPrizeID { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public RewardType RewardType { get; set; }
    }
}
