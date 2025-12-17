using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Logging;

namespace UserInfoWebApi.DynamoDB
{
    public class DynamoDbClientFactory : IDynamoDbClientFactory
    {
        private readonly object _locker = new object();
        private readonly ILogger<DynamoDbClientFactory> _logger;
        private Dictionary<RegionEndpoint, AmazonDynamoDBClient> _regionalDynamoDbClientMap = new Dictionary<RegionEndpoint, AmazonDynamoDBClient>();

        public DynamoDbClientFactory(ILogger<DynamoDbClientFactory> logger)
        {
            _logger = logger;
        }

        public AmazonDynamoDBClient CreateDynamoDbClient(string? region = null)
        {
            var regionEndpoint = string.IsNullOrEmpty(region) ? RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("REGION")) : RegionEndpoint.GetBySystemName(region);

            lock (_locker)
            {
                if (_regionalDynamoDbClientMap.TryGetValue(regionEndpoint, out var regionalDynamodbClient))
                {
                    return regionalDynamodbClient;
                }

                var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    LogResponse = true,
                    MaxErrorRetry = 1,
                    Timeout = TimeSpan.FromSeconds(10),
                    RegionEndpoint = regionEndpoint,
                });

                _regionalDynamoDbClientMap.Add(regionEndpoint, client);
                _logger.LogDebug($"DynamoDB client|{regionEndpoint.DisplayName}| is in use.");

                return client;
            }
        }
    }
}

