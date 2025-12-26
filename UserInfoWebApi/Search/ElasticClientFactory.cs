using System.Text;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;
using UserInfoWebApi.Model.Response;

namespace UserInfoWebApi.Search;

public class ElasticClientFactory:IElasticClientFactory
{
    private readonly ConnectionString _connStr;
    private readonly ILogger<ElasticClientFactory> _logger;
    private readonly IWebHostEnvironment _environment;

    public ElasticClientFactory(
        ConnectionString connStr,
        ILogger<ElasticClientFactory> logger,
        IWebHostEnvironment environment)
    {
        _connStr = connStr;
        _logger = logger;
        _environment = environment;
    }

    public IElasticClient CreateElasticClient()
    {
        var connectionPool = new SingleNodeConnectionPool(new Uri(_connStr.ToString()));
        var connSetting = new ConnectionSettings(connectionPool, new AwsHttpConnection())
            .DefaultMappingFor<User>(i => i.IndexName("user"))
            .DefaultMappingFor<Location>(i => i.IndexName("location"))
            .DefaultIndex("user");

        // fingers in ears, pretend we don't know this is wrong
        connSetting.ServerCertificateValidationCallback((_, _, _, _) => true);

        if (_environment.IsDevelopment())
        {
            connSetting.EnableDebugMode(details =>
            {
                _logger.LogDebug(details.DebugInformation);
                _logger.LogDebug($"Request Payload: {Encoding.UTF8.GetString(details.RequestBodyInBytes ?? Array.Empty<byte>())}");
            });
        }
        return new ElasticClient(connSetting);
    }
}