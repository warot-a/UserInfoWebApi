using Nest;
using Newtonsoft.Json;

namespace UserInfoWebApi.Model.Response;

[ElasticsearchType(RelationName = "user", IdProperty = nameof(Uuid))]
public class User
{
    public User()
    {
    }

    [Keyword]
    public string Uuid { get; set; }

    [JsonProperty(PropertyName = "firstname")]
    public string Firstname { get; set; }

    public string Lastname { get; set; }
    public string JobRole { get; set; }
    public string Email { get; set; }
    public string UserId { get; set; }
    public string AccountName { get; set; }
    public bool IsInternal { get; set; }

    [Keyword]
    public string LocationAccountId { get; set; }

    [Keyword]
    public string NearestLegalEntityId { get; set; }

    [Keyword]
    public string UltimateParentId { get; set; }

    public string PreferredLanguage { get; set; }
    public string LastSuccessLogin { get; set; }
    public string GeographicalFocus { get; set; }
    public string JobRoleCode { get; set; }
    public string AssetClassCode { get; set; }
    public string LastUpdatedByAAAOn { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}