namespace UserInfoWebApi.Model.Request;

public class SearchLocationRequest
{
    /// <summary>
    /// Gets or sets the keyword.
    /// </summary>
    /// <value>The keyword.</value>
    public string? Keyword { get; set; }

    /// <summary>
    /// Gets or sets the boundary filters for search.
    /// </summary>
    public IEnumerable<BoundaryFilterModel>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the limit.
    /// </summary>
    /// <value>The limit.</value>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <value>The offset.</value>
    public int Offset { get; set; } = 0;
}