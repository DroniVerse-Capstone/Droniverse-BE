using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.HttpClients
{
    public class CommunityMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CommunityMicroserviceClient> _logger;
        private readonly IDistributedCache _distributedCache; //Redis Cache
    }
}
