namespace Droniverse.Community.Application.DTO.Response
{
    public class UserGrowthTrendResponse
    {
        public List<MonthlyUserStat> UserGrowth { get; set; } = new();
    }

    public class MonthlyUserStat
    {
        public DateTime Month { get; set; }
        public int Value { get; set; }
    }
}
