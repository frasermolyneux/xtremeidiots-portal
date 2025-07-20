using Newtonsoft.Json;

namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Model representing the data posted by DataTables during AJAX requests for server-side processing
/// </summary>
public class DataTableAjaxPostModel
{
    /// <summary>
    /// Draw counter that DataTables uses to ensure that the AJAX returns correspond to the request
    /// </summary>
    [JsonProperty("draw")]
    public int Draw { get; set; }

    /// <summary>
    /// Paging first record indicator - starting point in the current data set
    /// </summary>
    [JsonProperty("start")]
    public int Start { get; set; }

    /// <summary>
    /// Number of records that the table can display in the current draw
    /// </summary>
    [JsonProperty("length")]
    public int Length { get; set; }

    /// <summary>
    /// Column definitions with ordering and search data
    /// </summary>
    [JsonProperty("columns")]
    public required List<DataTableColumn> Columns { get; set; }

    /// <summary>
    /// Global search criteria applied across all columns
    /// </summary>
    [JsonProperty("search")]
    public required DataTableSearch Search { get; set; }

    /// <summary>
    /// Ordering information for columns
    /// </summary>
    [JsonProperty("order")]
    public required List<DataTableOrder> Order { get; set; }
}