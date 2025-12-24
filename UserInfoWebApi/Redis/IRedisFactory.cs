using StackExchange.Redis;

namespace UserInfoWebApi.Redis;

public interface IRedisFactory
{
    IConnectionMultiplexer CreateConnection();
}