using System.Text.RegularExpressions;
using Nest;
using Newtonsoft.Json;
using StackExchange.Redis;
using UserInfoWebApi.Model.Request;
using UserInfoWebApi.Model.Response;
using UserInfoWebApi.Redis;
using UserInfoWebApi.Search;

namespace UserInfoWebApi.Repositories;

public partial class UserInfoRepository(IElasticClientFactory esFactory, IRedisFactory redisFactory)
    : IUserInfoRepository
{
    private readonly IElasticClient _esClient = esFactory.CreateElasticClient();
    private readonly IConnectionMultiplexer _redisConnection = redisFactory.CreateConnection();

    [GeneratedRegex("@\"[\\+\\-@!#%^<>:~*?\\\\/\\{\\}\\[\\]=&]\"")]
    private static partial Regex EscapeRegex();

    public async Task<User?> GetUserByUuidAsync(string uuid)
    {
        var result = await _esClient.SearchAsync<User>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Uuid.Suffix("keyword"))
                    .Value(uuid))));

        if (result.Hits.Count == 0)
        {
            return null;
        }

        return result.Documents.FirstOrDefault();
    }

    public async Task<IEnumerable<User>> GetUsersByUuidsAsync(IEnumerable<string> uuids)
    {
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return Enumerable.Empty<User>();
        }

        var result = await _esClient.SearchAsync<User>(s => s
            .From(0)
            .Size(uuidList.Count)
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.Uuid.Suffix("keyword"))
                    .Terms(uuidList))));

        return result.Documents;
    }

    public async Task<Location?> GetLocationByLocationIdAsync(string locationId)
    {
        var result = await _esClient.SearchAsync<Location>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.LocationId.Suffix("keyword"))
                    .Value(locationId))));

        return result.Documents.FirstOrDefault();
    }

    public async Task<IEnumerable<Location>> GetLocationsByLocationIdsAsync(IEnumerable<string> locationIds)
    {
        var locationIdList = locationIds.ToList();
        if (locationIdList.Count == 0)
        {
            return Enumerable.Empty<Location>();
        }

        var result = await _esClient.SearchAsync<Location>(s => s
            .From(0)
            .Size(locationIdList.Count)
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.LocationId.Suffix("keyword"))
                    .Terms(locationIdList))));

        return result.Documents;
    }

    public async Task<IEnumerable<User>> SearchUserAsync(SearchUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return Enumerable.Empty<User>();
        }

        if (request.SearchFields == null || !request.SearchFields.Any())
        {
            request.SearchFields = new[] { "firstname", "lastname", "email", "uuid", "userId" };
        }

        var fields = request.SearchFields
            .Select(f => new Field(f))
            .ToArray();

        var searchRequest = CreateSearchRequest<User>(
            fields,
            request.Keyword,
            request.Offset,
            request.Limit,
            request.Filters);

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            searchRequest.Sort = new List<ISort>
            {
                new FieldSort
                {
                    Field = $"{request.SortBy}.keyword",
                    Order = request.SortDirection == UserInfoSortDirection.DESC
                        ? SortOrder.Descending
                        : SortOrder.Ascending
                }
            };
        }

        var result = await _esClient.SearchAsync<User>(searchRequest);

        return result.Documents;
    }

    public async Task<IEnumerable<Location>> SearchLocationAsync(SearchLocationRequest request)
    {
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return Enumerable.Empty<Location>();
        }

        var fields = new[] { "name", "address", "city", "country", "locationId" }
            .Select(f => new Field(f))
            .ToArray();

        var searchRequest = CreateSearchRequest<Location>(
            fields,
            request.Keyword,
            request.Offset,
            request.Limit,
            request.Filters);
        var result = await _esClient.SearchAsync<Location>(searchRequest);

        return result.Documents;
    }

    public async Task<IEnumerable<UserAccount>> GetUserAccountsAsync(IEnumerable<string> uuids)
    {
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return Enumerable.Empty<UserAccount>();
        }

        var results = await GetRedisItems<RedisUserAccount>(uuidList);

        return results.Select(userAccount =>
        {
            if (userAccount == null)
            {
                return null;
            }

            return new UserAccount
            {
                Uuid = userAccount.Uuid,
                LocationAccountId = userAccount.LocationAccountId,
                NearestLegalEntityId = userAccount.NearestLegalEntityId,
                UltimateParentId = userAccount.UltimateParentId,
                IsInternal = userAccount.IsInternal,
                AccountName = userAccount.AccountName,
                Email = userAccount.Email,
                FirstName = userAccount.FirstName,
                JobRole = userAccount.JobRole,
                LastName = userAccount.LastName
            };
        }).Where(x => x != null)!;
    }

    public async Task<IEnumerable<LocationAccount>> GetLocationAccountsAsync(IEnumerable<string> locationIds)
    {
        var locationIdList = locationIds.ToList();
        if (locationIdList.Count == 0)
        {
            return Enumerable.Empty<LocationAccount>();
        }

        var results = await GetRedisItems<RedisLocationAccount>(locationIdList);

        return results.Select(locationAccount =>
        {
            if (locationAccount == null)
            {
                return null;
            }

            return new LocationAccount
            {
                LocationAccountId = locationAccount.LocationAccountId,
                NearestLegalEntityId = locationAccount.NearestLegalEntityId,
                UltimateParentId = locationAccount.UltimateParentId,
                IsInternal = locationAccount.IsInternal
            };
        }).Where(x => x != null)!;
    }

    private SearchRequest<T> CreateSearchRequest<T>(
        Field[] fields,
        string keyword,
        int offset,
        int limit,
        IEnumerable<BoundaryFilterModel>? filters)
    {
        var queryContainers = new List<QueryContainer>
        {
            new QueryStringQuery
            {
                Fields = fields,
                Type = TextQueryType.CrossFields,
                Query = ConvertSearchQueryString(keyword)
            }
        };

        if (filters != null && filters.Any())
        {
            foreach (var pair in filters)
            {
                queryContainers.Add(new TermsQuery
                {
                    Field = $"{pair.FieldName}.keyword",
                    Terms = pair.Values
                });
            }
        }

        var searchRequest = new SearchRequest<T>
        {
            From = offset,
            Size = limit,
            Query = new BoolQuery { Must = queryContainers }
        };

        return searchRequest;
    }

    private async Task<IEnumerable<T?>> GetRedisItems<T>(IList<string> idList)
    {
        var dbIndex = typeof(T) == typeof(RedisLocationAccount) ? 1 : 0;
        var redisDb = _redisConnection.GetDatabase(dbIndex);

        var tasks = idList.Select(uuid => redisDb.StringGetAsync(uuid)).ToList();

        var items = (await Task.WhenAll(tasks))
            .Where(redisValue => redisValue.HasValue)
            .Select(redisValue => JsonConvert.DeserializeObject<T>((string)redisValue));

        return items;
    }

    private string ConvertSearchQueryString(string originalQueryString)
    {
        var terms = originalQueryString.Split(' ').ToList();
        var convertedTerms = terms
            .Select(term => EscapeRegex().Replace(term, " AND "))
            .ToList();

        return $"(*{string.Join("*) AND (*", convertedTerms)}*)";
    }
}
