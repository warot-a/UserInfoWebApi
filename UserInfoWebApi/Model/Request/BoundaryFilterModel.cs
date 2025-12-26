namespace UserInfoWebApi.Model.Request;

public class BoundaryFilterModel
{
    public string FieldName { get; set; }

    public IEnumerable<string> Values { get; set; }
}