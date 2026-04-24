using System;

namespace Droniverse.Academy.Domain.Entities
{
    public class UserSimulator
    {
        public Guid UserSimulatorID { get; set; }
        public Guid UserID { get; set; }
        public Guid LessonID { get; set; }
        public DateTime SubmitAt { get; set; }
        public int FlightTime { get; set; } // seconds
        public int? Score { get; set; }
        public bool IsSuccess { get; set; }
        public Lesson Lesson { get; set; }
    }
}
