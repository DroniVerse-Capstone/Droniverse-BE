using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Job
{
    public class TestJob2
    {
        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("Test job 2 success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test job 2 failed: {ex.Message}");
                throw;
            }
        }
    }
}
