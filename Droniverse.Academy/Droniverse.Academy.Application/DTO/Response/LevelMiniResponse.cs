using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Response
{
    public class LevelMiniResponse
    {
        public Guid LevelID { get; set; }
        public int LevelNumber { get; set; }
        public string Name { get; set; }
    }
}
