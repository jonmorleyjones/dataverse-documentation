namespace DataverseDoc.Core;

/// <summary>
/// Exit codes used by the CLI application.
/// </summary>
public static class ExitCodes
{
    /// <summary>
    /// Command completed successfully.
    /// </summary>
    public const int Success = 0;

    /// <summary>
    /// General unspecified error.
    /// </summary>
    public const int GeneralError = 1;

    /// <summary>
    /// Authentication failed.
    /// </summary>
    public const int AuthenticationFailed = 2;

    /// <summary>
    /// Could not connect to Dataverse.
    /// </summary>
    public const int ConnectionError = 3;

    /// <summary>
    /// Invalid command line arguments.
    /// </summary>
    public const int InvalidArguments = 4;

    /// <summary>
    /// Requested resource was not found.
    /// </summary>
    public const int ResourceNotFound = 5;

    /// <summary>
    /// Access denied to the requested resource.
    /// </summary>
    public const int AccessDenied = 6;
}
