using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model representing a related player with enrichment (proxy + geo) data.
/// </summary>
public class RelatedPlayerEnrichedViewModel
{
    public Guid PlayerId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public int GameType { get; set; }

    // Enrichment
    public int? RiskScore { get; set; }
    public bool? IsProxy { get; set; }
    public bool? IsVpn { get; set; }
    public string? ProxyType { get; set; }
    public string? CountryCode { get; set; }

    public static RelatedPlayerEnrichedViewModel FromRelatedPlayerDto(object relatedPlayerDto)
    {
        // We don't have the concrete type source here; map via dynamic to keep decoupled.
        dynamic d = relatedPlayerDto;
        return new RelatedPlayerEnrichedViewModel
        {
            PlayerId = d.PlayerId,
            Username = d.Username,
            IpAddress = d.IpAddress,
            GameType = (int)d.GameType
        };
    }
}
