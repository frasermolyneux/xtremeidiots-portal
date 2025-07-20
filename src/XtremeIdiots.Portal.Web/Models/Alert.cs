namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents an alert message to be displayed to users
/// </summary>
/// <remarks>
/// Initializes a new instance of the Alert class
/// </remarks>
/// <param name="message">The message to display</param>
/// <param name="type">The alert type (e.g., "alert-success", "alert-danger")</param>
/// <exception cref="ArgumentNullException">Thrown when message or type is null</exception>
public class Alert(string message, string type)
{
    /// <summary>
    /// Gets the alert message text
    /// </summary>
    public string Message { get; } = message ?? throw new ArgumentNullException(nameof(message));

    /// <summary>
    /// Gets the alert type (Bootstrap CSS class)
    /// </summary>
    public string Type { get; } = type ?? throw new ArgumentNullException(nameof(type));
}