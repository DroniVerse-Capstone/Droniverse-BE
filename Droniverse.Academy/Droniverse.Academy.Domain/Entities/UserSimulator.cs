using System;

namespace Droniverse.Academy.Domain.Entities
{
    public class UserSimulator
    {
        public Guid UserSimulatorID { get; set; }
        public Guid UserLessonID { get; set; }
        public int FlightTime { get; set; } // seconds
        public int? Score { get; set; }
        public bool IsSuccess { get; set; }

        // Navigation properties (optional, if you have related entities)
        public UserLesson UserLesson { get; set; }
    }
}
