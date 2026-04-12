using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public class GetAllCodesByClubSearchRequest : SearchRequest
    {
        public CodeState? CodeState { get; set; }
    }

    public enum CodeState
    {
        UnUse,
        Used
    }
}
