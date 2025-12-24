using Nest;

namespace UserInfoWebApi.Model.Response;

[ElasticsearchType(RelationName = "location", IdProperty = nameof(LocationId))]
public class Location
{
    public Location()
    {
    }

    [Keyword]
    public string LocationId { get; set; }

    [Keyword]
    public LocationType Type { get; set; }

    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    [Keyword]
    public string NearestLegalEntityId { get; set; }

    [Keyword]
    public string UltimateParentId { get; set; }

    public bool IsInternal { get; set; }
}