using StackExchange.Redis;

namespace UserInfoWebApi.Redis;

public class RedisFactory : IRedisFactory
{
    private readonly string _configuration;
    private IConnectionMultiplexer? _connection;

    public RedisFactory(string configuration)
    {
        _configuration = configuration;
    }

    public IConnectionMultiplexer CreateConnection()
    {
        if (_connection != null && !_connection.IsConnected)
        {
            _connection.Dispose();
            _connection = null;
        }

        if (_connection == null)
        {
            var options = ConfigurationOptions.Parse(_configuration);
            // fingers in ears, pretend we don't know this is wrong
            options.CertificateValidation += (_, _, _, _) => true;
            options.SyncTimeout = 30000;

            _connection = ConnectionMultiplexer.Connect(options);
        }

        return _connection;
    }
}