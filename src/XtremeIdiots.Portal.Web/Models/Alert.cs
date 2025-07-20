namespace XtremeIdiots.Portal.Web.Models;

/// <summary>
/// Represents an alert message to be displayed to users
/// </summary>
public class Alert
{
    /// <summary>
    /// Gets the alert message text
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the alert type (Bootstrap CSS class)
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Initializes a new instance of the Alert class
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="type">The alert type (e.g., "alert-success", "alert-danger")</param>
    /// <exception cref="ArgumentNullException">Thrown when message or type is null</exception>
    public Alert(string message, string type)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }
}