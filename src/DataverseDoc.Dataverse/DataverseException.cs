using System.Net;

namespace DataverseDoc.Dataverse;

/// <summary>
/// Exception thrown when a Dataverse API call fails.
/// </summary>
public class DataverseException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The error code from Dataverse (if available).
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// The request URI that failed.
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    /// Creates a new DataverseException.
    /// </summary>
    public DataverseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new DataverseException with status code.
    /// </summary>
    public DataverseException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates a new DataverseException with full details.
    /// </summary>
    public DataverseException(
        string message,
        HttpStatusCode statusCode,
        string? errorCode,
        Uri? requestUri,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RequestUri = requestUri;
    }
}
