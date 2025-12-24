using System.Net;
using Amazon.CloudWatchLogs.Model;
using Amazon.DynamoDBv2;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using StackExchange.Redis;
using UserInfoWebApi.DynamoDB;
using UserInfoWebApi.Redis;
using UserInfoWebApi.Search;

namespace UserInfoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public class HealthCheckController : ControllerBase
{
    private readonly IElasticClient _ecClient;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly IConnectionMultiplexer _redisConnection;

    public HealthCheckController(
        IElasticClientFactory esFactory,
        IRedisFactory redisFactory,
        IDynamoDbClientFactory dynamoDbClientFactory)
    {
        _ecClient = esFactory.CreateElasticClient();
        _redisConnection = redisFactory.CreateConnection();
        _dynamoDbClient = dynamoDbClientFactory.CreateDynamoDbClient();
    }

    [HttpGet]
    public async Task<ObjectResult> HealthCheck(
        [FromQuery] bool searchError = false, // For testing: if true, it pretends OpenSearch connection error
        [FromQuery] bool redisError = false, // For testing: if true, it pretends Redis connection error
        [FromQuery] bool dynamoError = false // For testing: if true, it pretends DynamoDB connection error
    )
    {
        try
        {
            var searchHealthy = await IsElasticSearchHealthy(searchError);
            var redisConnection = IsRedisHealthy(redisError);
            var dynamoDbHealthy = await IsDynamoDbHealthy(dynamoError);
            return Ok(new
            {
                Status = "OK",
                Environment = Environment.GetEnvironmentVariable("ENVIRONMENT"),
                Region = Environment.GetEnvironmentVariable("REGION"),
                SearchHealthy = searchHealthy,
                RedisConnection = redisConnection,
                DynamoDbHealthy = dynamoDbHealthy
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);

            throw new ServiceUnavailableException(e.Message);
        }
    }

    private async Task<bool> IsElasticSearchHealthy(bool searchError)
    {
        var response = await _ecClient.Cluster.HealthAsync();
        var isHealthy = response.Status == Health.Green || response.Status == Health.Yellow;
        if (searchError || !isHealthy)
        {
            throw new ServiceUnavailableException("OpenSearch is unavailable");
        }

        return isHealthy;
    }

    private bool IsRedisHealthy(bool redisError)
    {
        var isHealthy = _redisConnection.IsConnected;
        if (redisError || !isHealthy)
        {
            throw new ServiceUnavailableException("Redis is unavailable");
        }

        return isHealthy;
    }

    private async Task<bool> IsDynamoDbHealthy(bool dynamoError)
    {
        var result = await _dynamoDbClient.DescribeTableAsync(Environment.GetEnvironmentVariable("APPLICATION_TABLE_NAME"));
        var isHealthy = result.HttpStatusCode == HttpStatusCode.OK;
        if (dynamoError || !isHealthy)
        {
            throw new ServiceUnavailableException("DynamoDB is unavailable");
        }

        return isHealthy;
    }
}