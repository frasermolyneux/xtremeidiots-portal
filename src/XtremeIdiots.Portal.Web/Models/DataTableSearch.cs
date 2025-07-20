using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents search parameters for DataTables component
/// </summary>
public class DataTableSearch
{
    /// <summary>
    /// Gets or sets the search value entered by the user
    /// </summary>
    [JsonProperty("value")]
    public required string Value { get; set; }

    /// <summary>
    /// Gets or sets whether the search value should be treated as a regular expression
    /// </summary>
    [JsonProperty("regex")]
    public required string Regex { get; set; }
}