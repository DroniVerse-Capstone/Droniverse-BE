using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs
{
    public class LabRuleResponse
    {
        public int TimeLimit { get; set; }
        public int RequiredScore { get; set; }
        public int MaxBlocks { get; set; }
        public bool SequentialCheckpoints { get; set; }
    }
}
