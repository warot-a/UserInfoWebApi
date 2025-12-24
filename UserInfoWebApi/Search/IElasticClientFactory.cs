using Nest;

namespace UserInfoWebApi.Search;

public interface IElasticClientFactory
{
    IElasticClient CreateElasticClient();
}