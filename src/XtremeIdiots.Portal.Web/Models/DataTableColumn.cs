using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents a column configuration for DataTables AJAX requests
/// </summary>
public class DataTableColumn
{
    /// <summary>
    /// Gets or sets the data source for the column
    /// </summary>
    [JsonProperty("data")]
    public required string Data { get; set; }

    /// <summary>
    /// Gets or sets the name of the column
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the column is searchable
    /// </summary>
    [JsonProperty("searchable")]
    public required string Searchable { get; set; }

    /// <summary>
    /// Gets or sets whether the column is orderable (sortable)
    /// </summary>
    [JsonProperty("orderable")]
    public required string Orderable { get; set; }

    /// <summary>
    /// Gets or sets the search configuration for this column
    /// </summary>
    [JsonProperty("search")]
    public required DataTableSearch Search { get; set; }
}