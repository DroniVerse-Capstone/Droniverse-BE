using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.Entities
{
    public class ClubCreationRequestCategory
    {
        public Guid ClubCreationRequestID { get; set; }
        public Guid CategoryID { get; set; }

        public ClubCreationRequest ClubCreationRequest { get; set; }
        public Category Category { get; set; }
    }
}
