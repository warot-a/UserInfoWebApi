namespace UserInfoWebApi.Model.Response;

public class LocationAccount
{
    public LocationAccount()
    {
    }

    public string LocationAccountId { get; set; }
    public string NearestLegalEntityId { get; set; }
    public string UltimateParentId { get; set; }
    public string IsInternal { get; set; }
}