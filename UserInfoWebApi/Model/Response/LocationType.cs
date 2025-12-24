using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UserInfoWebApi.Model.Response;

[JsonConverter(typeof(StringEnumConverter))]
public enum LocationType
{
    All = 0,
    LOC = 1, // Location
    LGL = 2, // LegalEntity
    ULT = 3 // UltimateParent
}