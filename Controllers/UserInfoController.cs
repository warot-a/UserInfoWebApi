using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using StackExchange.Redis;
using UserInfoWebApi.AuthenticationHandlers;
using UserInfoWebApi.Model;
using UserInfoWebApi.Model.Request;
using UserInfoWebApi.Model.Response;
using UserInfoWebApi.Redis;
using UserInfoWebApi.Search;

namespace UserInfoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = AuthenticationConstants.ApplicationIdAuthentication)]
public class UserInfoController : ControllerBase
{
    private readonly ILogger<UserInfoController> _logger;
    private readonly IElasticClient _esClient;
    private readonly IConnectionMultiplexer _redisConnection;
    private const string ErrorKeywordRequired = "Property \"keyword\" is required";
    private const string ErrorNonEmptyArray = "A non-empty array of string is required";


    public UserInfoController(
        ILogger<UserInfoController> logger,
        IElasticClientFactory esFactory,
        IRedisFactory redisFactory)
    {
        _logger = logger;
        _esClient = esFactory.CreateElasticClient();
        _redisConnection = redisFactory.CreateConnection();
    }

    [HttpGet("getUserByUuid/{uuid}")]
    [ProducesResponseType(200, Type = typeof(User))]
    public async Task<ObjectResult> GetUserByUuidAsync(string uuid)
    {
        _logger.LogInformation("Start GET getUserByUuid");
        var result = await _esClient.SearchAsync<User>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.Uuid.Suffix("keyword"))
                    .Value(uuid))));

        if (result.Hits.Count == 0)
        {
            return NotFound($"Cannot find user {uuid}");
        }

        return Ok(result.Documents.FirstOrDefault());
    }

    [HttpPost("getUsersByUuids")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
    public async Task<ObjectResult> GetUsersByUuidsAsync([FromBody] IEnumerable<string> uuids)
    {
        _logger.LogInformation("Start POST getUserByUuid");
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var result = await _esClient.SearchAsync<User>(s => s
            .From(0)
            .Size(uuidList.Count)
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.Uuid.Suffix("keyword"))
                    .Terms(uuidList))));

        return Ok(result.Documents);
    }

    [HttpGet("getLocationByLocationId/{locationId}")]
    [ProducesResponseType(200, Type = typeof(Location))]
    public async Task<ObjectResult> GetLocationByLocationIdAsync(string locationId)
    {
        _logger.LogInformation("Start GET getLocationByLocationId");
        var result = await _esClient.SearchAsync<Location>(s => s
            .Query(q => q
                .Term(t => t
                    .Field(f => f.LocationId.Suffix("keyword"))
                    .Value(locationId))));

        return Ok(result.Documents.FirstOrDefault());
    }

    [HttpPost("getLocationsByLocationIds")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
    public async Task<ObjectResult> GetLocationsByLocationIdsAsync([FromBody] IEnumerable<string> locationIds)
    {
        _logger.LogInformation("Start POST getLocationsByLocationIds");
        var locationIdList = locationIds.ToList();
        if (locationIdList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var result = await _esClient.SearchAsync<Location>(s => s
            .From(0)
            .Size(locationIdList.Count)
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.LocationId.Suffix("keyword"))
                    .Terms(locationIdList))));

        return Ok(result.Documents);
    }

    [HttpPost("searchUser")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
    public async Task<ObjectResult> SearchUserAsync([FromBody] SearchUserRequest request)
    {
        _logger.LogInformation("Start POST searchUser");
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return UnprocessableEntity(new CommonError { Message = ErrorKeywordRequired });
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
                    Order = request.SortDirection == UserInfoSortDirection.DESC ? SortOrder.Descending : SortOrder.Ascending
                }
            };
        }

        var result = await _esClient.SearchAsync<User>(searchRequest);

        return Ok(result.Documents);
    }

    [HttpPost("searchLocation")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
    public async Task<ObjectResult> SearchLocationAsync([FromBody] SearchLocationRequest request)
    {
        _logger.LogInformation("Start POST searchLocation");
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return UnprocessableEntity(new CommonError { Message = ErrorKeywordRequired });
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

        return Ok(result.Documents);
    }

    [HttpPost("getUserAccounts")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserAccount>))]
    public async Task<ObjectResult> GetUserAccountsAsync([FromBody] IEnumerable<string> uuids)
    {
        _logger.LogInformation("Start POST getUserAccounts");
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var results = await GetRedisItems<RedisUserAccount>(uuidList);

        return Ok(results.Select(userAccount =>
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
        }));
    }

    [HttpPost("getLocationAccounts")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<LocationAccount>))]
    public async Task<ObjectResult> GetLocationAccountsAsync([FromBody] IEnumerable<string> locationIds)
    {
        _logger.LogInformation("Start POST getLocationAccounts");
        var locationIdList = locationIds.ToList();
        if (!locationIdList.Any())
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var results = await GetRedisItems<RedisLocationAccount>(locationIdList);

        return Ok(results.Select(locationAccount =>
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
        }));
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
        var convertedTerms = new List<string>();
        var terms = originalQueryString.Split(' ').ToList();
        var escapeChars = new List<string>
        {
            "+", "-", "@", "!", "#", "%", "^", "<", ">", ":", "~", "*", "?", "\\", "/", "{", "}", "[", "]", "=", "&"
        };
        terms.ForEach(e =>
        {
            escapeChars.ForEach(escapeChar => { e = e.Replace(escapeChar, " AND "); });
            convertedTerms.Add(e);
        });

        var queryString = $"(*{string.Join("*) AND (*", convertedTerms)}*)";
        return queryString;
    }
}