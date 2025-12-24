using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserInfoWebApi.AuthenticationHandlers;
using UserInfoWebApi.Model;
using UserInfoWebApi.Model.Request;
using UserInfoWebApi.Model.Response;
using UserInfoWebApi.Repositories;

namespace UserInfoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = AuthenticationConstants.ApplicationIdAuthentication)]
public class UserInfoController(ILogger<UserInfoController> logger, IUserInfoRepository repository)
    : ControllerBase
{
    private const string ErrorKeywordRequired = "Property \"keyword\" is required";
    private const string ErrorNonEmptyArray = "A non-empty array of string is required";


    [HttpGet("getUserByUuid/{uuid}")]
    [ProducesResponseType(200, Type = typeof(User))]
    public async Task<ObjectResult> GetUserByUuidAsync(string uuid)
    {
        logger.LogInformation("Start GET getUserByUuid");
        var user = await repository.GetUserByUuidAsync(uuid);

        if (user == null)
        {
            return NotFound($"Cannot find user {uuid}");
        }

        return Ok(user);
    }

    [HttpPost("getUsersByUuids")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
    public async Task<ObjectResult> GetUsersByUuidsAsync([FromBody] IEnumerable<string> uuids)
    {
        logger.LogInformation("Start POST getUserByUuid");
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var users = await repository.GetUsersByUuidsAsync(uuidList);

        return Ok(users);
    }

    [HttpGet("getLocationByLocationId/{locationId}")]
    [ProducesResponseType(200, Type = typeof(Location))]
    public async Task<ObjectResult> GetLocationByLocationIdAsync(string locationId)
    {
        logger.LogInformation("Start GET getLocationByLocationId");
        var location = await repository.GetLocationByLocationIdAsync(locationId);

        if (location == null)
        {
            return NotFound($"Cannot find location {locationId}");
        }

        return Ok(location);
    }

    [HttpPost("getLocationsByLocationIds")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
    public async Task<ObjectResult> GetLocationsByLocationIdsAsync([FromBody] IEnumerable<string> locationIds)
    {
        logger.LogInformation("Start POST getLocationsByLocationIds");
        var locationIdList = locationIds.ToList();
        if (locationIdList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var locations = await repository.GetLocationsByLocationIdsAsync(locationIdList);

        return Ok(locations);
    }

    [HttpPost("searchUser")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
    public async Task<ObjectResult> SearchUserAsync([FromBody] SearchUserRequest request)
    {
        logger.LogInformation("Start POST searchUser");
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return UnprocessableEntity(new CommonError { Message = ErrorKeywordRequired });
        }


        var users = await repository.SearchUserAsync(request);

        return Ok(users);
    }

    [HttpPost("searchLocation")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Location>))]
    public async Task<ObjectResult> SearchLocationAsync([FromBody] SearchLocationRequest request)
    {
        logger.LogInformation("Start POST searchLocation");
        if (string.IsNullOrEmpty(request.Keyword))
        {
            return UnprocessableEntity(new CommonError { Message = ErrorKeywordRequired });
        }

        var locations = await repository.SearchLocationAsync(request);

        return Ok(locations);
    }

    [HttpPost("getUserAccounts")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserAccount>))]
    public async Task<ObjectResult> GetUserAccountsAsync([FromBody] IEnumerable<string> uuids)
    {
        logger.LogInformation("Start POST getUserAccounts");
        var uuidList = uuids.ToList();
        if (uuidList.Count == 0)
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var userAccounts = await repository.GetUserAccountsAsync(uuidList);

        return Ok(userAccounts);
    }

    [HttpPost("getLocationAccounts")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<LocationAccount>))]
    public async Task<ObjectResult> GetLocationAccountsAsync([FromBody] IEnumerable<string> locationIds)
    {
        logger.LogInformation("Start POST getLocationAccounts");
        var locationIdList = locationIds.ToList();
        if (!locationIdList.Any())
        {
            return UnprocessableEntity(new CommonError { Message = ErrorNonEmptyArray });
        }

        var locationAccounts = await repository.GetLocationAccountsAsync(locationIdList);

        return Ok(locationAccounts);
    }
}
