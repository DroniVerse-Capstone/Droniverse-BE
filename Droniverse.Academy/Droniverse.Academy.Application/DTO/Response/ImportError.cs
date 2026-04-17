using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.DTO.Response
{
    public class ImportError
    {
        public int Row { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
