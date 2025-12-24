using UserInfoWebApi.Model.Request;
using UserInfoWebApi.Model.Response;

namespace UserInfoWebApi.Repositories;

public interface IUserInfoRepository
{
    Task<User?> GetUserByUuidAsync(string uuid);
    Task<IEnumerable<User>> GetUsersByUuidsAsync(IEnumerable<string> uuids);
    Task<Location?> GetLocationByLocationIdAsync(string locationId);
    Task<IEnumerable<Location>> GetLocationsByLocationIdsAsync(IEnumerable<string> locationIds);
    Task<IEnumerable<User>> SearchUserAsync(SearchUserRequest request);
    Task<IEnumerable<Location>> SearchLocationAsync(SearchLocationRequest request);
    Task<IEnumerable<UserAccount>> GetUserAccountsAsync(IEnumerable<string> uuids);
    Task<IEnumerable<LocationAccount>> GetLocationAccountsAsync(IEnumerable<string> locationIds);
}
