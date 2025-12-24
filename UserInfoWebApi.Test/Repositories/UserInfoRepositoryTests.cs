using Moq;
using Nest;
using StackExchange.Redis;
using UserInfoWebApi.Model.Response;
using UserInfoWebApi.Redis;
using UserInfoWebApi.Repositories;
using UserInfoWebApi.Search;
using Newtonsoft.Json;
using UserInfoWebApi.Model.Request;

namespace UserInfoWebApi.Test.Repositories;

public class UserInfoRepositoryTests
{
    private readonly Mock<IElasticClient> _mockEsClient;
    private readonly Mock<IConnectionMultiplexer> _mockRedisConnection;
    private readonly UserInfoRepository _repository;

    public UserInfoRepositoryTests()
    {
        var mockEsFactory = new Mock<IElasticClientFactory>();
        var mockRedisFactory = new Mock<IRedisFactory>();
        _mockEsClient = new Mock<IElasticClient>();
        _mockRedisConnection = new Mock<IConnectionMultiplexer>();

        mockEsFactory
            .Setup(x => x.CreateElasticClient())
            .Returns(_mockEsClient.Object);
        mockRedisFactory
            .Setup(x => x.CreateConnection())
            .Returns(_mockRedisConnection.Object);

        _repository = new UserInfoRepository(mockEsFactory.Object, mockRedisFactory.Object);
    }

    [Fact]
    public async Task GetUserByUuidAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var uuid = "test-uuid";
        var user = new User { Uuid = uuid, Firstname = "Test", Lastname = "User" };
        var searchResponse = new Mock<ISearchResponse<User>>();

        searchResponse
            .Setup(x => x.Documents)
            .Returns(new List<User> { user });
        var mockHit = new Mock<IHit<User>>();
        searchResponse
            .Setup(x => x.Hits)
            .Returns(new List<IHit<User>> { mockHit.Object });

        _mockEsClient
            .Setup(x =>
                x.SearchAsync(It.IsAny<Func<SearchDescriptor<User>, ISearchRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetUserByUuidAsync(uuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(uuid, result.Uuid);
    }

    [Fact]
    public async Task GetUserByUuidAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var uuid = "non-existent-uuid";
        var searchResponse = new Mock<ISearchResponse<User>>();

        searchResponse
            .Setup(x => x.Documents)
            .Returns(new List<User>());
        searchResponse
            .Setup(x => x.Hits)
            .Returns(new List<IHit<User>>());

        _mockEsClient
            .Setup(x =>
                x.SearchAsync(It.IsAny<Func<SearchDescriptor<User>, ISearchRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetUserByUuidAsync(uuid);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUsersByUuidsAsync_ShouldReturnUsers_WhenUuidsExist()
    {
        // Arrange
        var uuids = new[] { "uuid1", "uuid2" };
        var users = new List<User> { new() { Uuid = "uuid1" }, new() { Uuid = "uuid2" } };
        var searchResponse = new Mock<ISearchResponse<User>>();
        searchResponse.Setup(r => r.Documents).Returns(users);

        _mockEsClient
            .Setup(c => c.SearchAsync(It.IsAny<Func<SearchDescriptor<User>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetUsersByUuidsAsync(uuids);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUsersByUuidsAsync_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var uuids = Enumerable.Empty<string>();

        // Act
        var result = await _repository.GetUsersByUuidsAsync(uuids);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLocationByLocationIdAsync_ShouldReturnLocation_WhenLocationExists()
    {
        // Arrange
        var locationId = "loc1";
        var location = new Location { LocationId = locationId };
        var searchResponse = new Mock<ISearchResponse<Location>>();
        searchResponse.Setup(r => r.Documents).Returns(new[] { location });

        _mockEsClient
            .Setup(c => c.SearchAsync(It.IsAny<Func<SearchDescriptor<Location>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetLocationByLocationIdAsync(locationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(locationId, result.LocationId);
    }

    [Fact]
    public async Task GetLocationByLocationIdAsync_ShouldReturnNull_WhenLocationDoesNotExist()
    {
        // Arrange
        var locationId = "loc1";
        var searchResponse = new Mock<ISearchResponse<Location>>();
        searchResponse
            .Setup(r => r.Documents)
            .Returns((IReadOnlyCollection<Location>)Enumerable.Empty<Location>());

        _mockEsClient
            .Setup(c => c.SearchAsync(It.IsAny<Func<SearchDescriptor<Location>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetLocationByLocationIdAsync(locationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLocationsByLocationIdsAsync_ShouldReturnLocations_WhenLocationIdsExist()
    {
        // Arrange
        var locationIds = new[] { "loc1", "loc2" };
        var locations = new List<Location> { new() { LocationId = "loc1" }, new() { LocationId = "loc2" } };
        var searchResponse = new Mock<ISearchResponse<Location>>();
        searchResponse.Setup(r => r.Documents).Returns(locations);

        _mockEsClient
            .Setup(c => c.SearchAsync(It.IsAny<Func<SearchDescriptor<Location>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.GetLocationsByLocationIdsAsync(locationIds);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetLocationsByLocationIdsAsync_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var locationIds = Enumerable.Empty<string>();

        // Act
        var result = await _repository.GetLocationsByLocationIdsAsync(locationIds);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchUserAsync_ShouldReturnUsers_WhenKeywordMatches()
    {
        // Arrange
        var request = new SearchUserRequest { Keyword = "test" };
        var users = new List<User> { new() { Firstname = "test" } };
        var searchResponse = new Mock<ISearchResponse<User>>();
        searchResponse.Setup(r => r.Documents).Returns(users);

        _mockEsClient
            .Setup(c => c.SearchAsync<User>(It.IsAny<ISearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.SearchUserAsync(request);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchUserAsync_ShouldReturnEmpty_WhenKeywordIsEmpty()
    {
        // Arrange
        var request = new SearchUserRequest { Keyword = "" };

        // Act
        var result = await _repository.SearchUserAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchLocationAsync_ShouldReturnLocations_WhenKeywordMatches()
    {
        // Arrange
        var request = new SearchLocationRequest { Keyword = "test" };
        var locations = new List<Location> { new() { Name = "test location" } };
        var searchResponse = new Mock<ISearchResponse<Location>>();
        searchResponse.Setup(r => r.Documents).Returns(locations);

        _mockEsClient
            .Setup(c => c.SearchAsync<Location>(It.IsAny<ISearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse.Object);

        // Act
        var result = await _repository.SearchLocationAsync(request);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchLocationAsync_ShouldReturnEmpty_WhenKeywordIsEmpty()
    {
        // Arrange
        var request = new SearchLocationRequest { Keyword = "" };

        // Act
        var result = await _repository.SearchLocationAsync(request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserAccountsAsync_ShouldReturnAccounts_WhenAccountsExistInRedis()
    {
        // Arrange
        var uuids = new[] { "uuid1" };
        var userAccount = new RedisUserAccount { Uuid = "uuid1" };
        var redisValue = new RedisValue(JsonConvert.SerializeObject(userAccount));
        var mockDb = new Mock<IDatabase>();
        mockDb
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);
        _mockRedisConnection
            .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDb.Object);

        // Act
        var result = await _repository.GetUserAccountsAsync(uuids);

        // Assert
        Assert.Single(result);
        Assert.Equal("uuid1", result.First().Uuid);
    }

    [Fact]
    public async Task GetUserAccountsAsync_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var uuids = Enumerable.Empty<string>();

        // Act
        var result = await _repository.GetUserAccountsAsync(uuids);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLocationAccountsAsync_ShouldReturnAccounts_WhenAccountsExistInRedis()
    {
        // Arrange
        var locationIds = new[] { "loc1" };
        var locationAccount = new RedisLocationAccount { LocationAccountId = "loc1" };
        var redisValue = new RedisValue(JsonConvert.SerializeObject(locationAccount));
        var mockDb = new Mock<IDatabase>();
        mockDb
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);
        _mockRedisConnection
            .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDb.Object);

        // Act
        var result = await _repository.GetLocationAccountsAsync(locationIds);

        // Assert
        Assert.Single(result);
        Assert.Equal("loc1", result.First().LocationAccountId);
    }

    [Fact]
    public async Task GetLocationAccountsAsync_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Arrange
        var locationIds = Enumerable.Empty<string>();

        // Act
        var result = await _repository.GetLocationAccountsAsync(locationIds);

        // Assert
        Assert.Empty(result);
    }
}