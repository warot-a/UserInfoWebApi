using Microsoft.Extensions.DependencyInjection;
using UserInfoWebApi.Redis;
using ConnectionString = UserInfoWebApi.Search.ConnectionString;
using ElasticClientFactory = UserInfoWebApi.Search.ElasticClientFactory;
using IElasticClientFactory = UserInfoWebApi.Search.IElasticClientFactory;
using DynamoDbClientFactory = UserInfoWebApi.DynamoDB.DynamoDbClientFactory;
using IDynamoDbClientFactory = UserInfoWebApi.DynamoDB.IDynamoDbClientFactory;

namespace UserInfoWebApi.ServiceFactory;

public static class ServicesConfiguration
{
    public static void AddUserInfoServiceDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ConnectionString>(_ => new ConnectionString
        {
            Hostname = Environment.GetEnvironmentVariable("ESEndpoint") ?? string.Empty,
            Secure = Environment.GetEnvironmentVariable("ESProtocol") == "https"
        });
        services.AddSingleton<IElasticClientFactory, ElasticClientFactory>();
        services.AddSingleton<IRedisFactory>(_ => new RedisFactory(Environment.GetEnvironmentVariable("REDIS_CONFIG_1")));
        services.AddSingleton<IDynamoDbClientFactory, DynamoDbClientFactory>();
    }
}