namespace Droniverse.Community.Domain.Entities
{
    public class CompetitionLevel
    {
        public Guid CompetitionID { get; set; }
        public Guid LevelID { get; set; }
        public Competition Competition { get; set; }
    }
}
