using Newtonsoft.Json;

namespace UserInfoWebApi.Model.Response;

[JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
public class RedisLocationAccount
{
    public RedisLocationAccount()
    {
    }

    [JsonProperty(PropertyName = "AccId")]
    public string LocationAccountId { get; set; }

    [JsonProperty(PropertyName = "nleId")]
    public string NearestLegalEntityId { get; set; }

    [JsonProperty(PropertyName = "upId")]
    public string UltimateParentId { get; set; }

    [JsonProperty(PropertyName = "isInternal")]
    public string IsInternal { get; set; }
}