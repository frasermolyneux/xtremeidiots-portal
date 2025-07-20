using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents ordering information for DataTables AJAX requests
/// </summary>
public class DataTableOrder
{
    /// <summary>
    /// Gets or sets the column index to order by
    /// </summary>
    [JsonProperty("column")]
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the direction to order by (asc/desc)
    /// </summary>
    [JsonProperty("dir")]
    public required string Dir { get; set; }
}