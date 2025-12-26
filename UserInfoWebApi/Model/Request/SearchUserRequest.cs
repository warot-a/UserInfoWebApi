namespace UserInfoWebApi.Model.Request;

public class SearchUserRequest
{
    /// <summary>
    /// Gets or sets the keyword.
    /// </summary>
    /// <value>The keyword.</value>
    public string? Keyword { get; set; }

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

    /// <summary>
    /// Gets or sets the name of the fields which are used for searching.
    /// </summary>
    public IEnumerable<string>? SearchFields { get; set; }

    /// <summary>
    /// Gets or sets the boundary filters for search.
    /// </summary>
    public IEnumerable<BoundaryFilterModel>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the field specific to sort the result-set.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction to sort the result-set in ascending or descending order based on specific sortBy field.
    /// Ascending order by default.
    /// </summary>
    public UserInfoSortDirection? SortDirection { get; set; }
}