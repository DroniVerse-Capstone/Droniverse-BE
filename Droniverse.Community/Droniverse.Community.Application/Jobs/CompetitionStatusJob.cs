using Droniverse.Community.Application.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Jobs
{
    public class CompetitionStatusJob
    {
        private readonly ICompetitionService _competitionService;

        public CompetitionStatusJob(ICompetitionService competitionService)
        {
            _competitionService = competitionService;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("CompetitionStatusJob started");
                await _competitionService.UpdateCompetitionStatusesAsync();
                Console.WriteLine("CompetitionStatusJob finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CompetitionStatusJob failed: {ex.Message}");
                throw;
            }
        }
    }
}
