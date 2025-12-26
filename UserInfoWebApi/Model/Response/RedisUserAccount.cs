using Newtonsoft.Json;

namespace UserInfoWebApi.Model.Response;

[JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
public class RedisUserAccount
{
    public RedisUserAccount()
    {
    }

    [JsonProperty(PropertyName = "Id")]
    public string Uuid { get; set; }

    [JsonProperty(PropertyName = "AccId")]
    public string LocationAccountId { get; set; }

    [JsonProperty(PropertyName = "NleId")]
    public string NearestLegalEntityId { get; set; }

    [JsonProperty(PropertyName = "UpId")]
    public string UltimateParentId { get; set; }

    [JsonProperty(PropertyName = "IsInternal")]
    public bool IsInternal { get; set; }

    [JsonProperty(PropertyName = "fn")]
    public string FirstName { get; set; }

    [JsonProperty(PropertyName = "sn")]
    public string LastName { get; set; }

    [JsonProperty(PropertyName = "mail")]
    public string Email { get; set; }

    [JsonProperty(PropertyName = "jr")]
    public string JobRole { get; set; }

    [JsonProperty(PropertyName = "an")]
    public string AccountName { get; set; }
}