using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Jobs
{
    public class TestJob
    {
        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("Test job success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test job failed: {ex.Message}");
                throw;
            }
        }
    }
}
