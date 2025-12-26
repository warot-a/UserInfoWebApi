namespace UserInfoWebApi.Model.Request;

public class GetUsersByUuidsRequest
{
    public IEnumerable<string> Uuids { get; set; }
}