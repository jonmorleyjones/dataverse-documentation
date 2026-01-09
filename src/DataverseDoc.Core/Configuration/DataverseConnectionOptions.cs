namespace DataverseDoc.Core.Configuration;

/// <summary>
/// Authentication mode for connecting to Dataverse.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// Interactive authentication using device code or browser login.
    /// </summary>
    Interactive,

    /// <summary>
    /// Service principal authentication using client credentials.
    /// </summary>
    ServicePrincipal
}

/// <summary>
/// Configuration options for connecting to a Dataverse environment.
/// </summary>
public class DataverseConnectionOptions
{
    /// <summary>
    /// The URL of the Dataverse environment (e.g., https://org.crm.dynamics.com).
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The authentication mode to use.
    /// </summary>
    public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.Interactive;

    /// <summary>
    /// The Azure AD tenant ID.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// The application (client) ID for authentication.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The client secret for service principal authentication.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// The path to the certificate file for certificate-based authentication.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// The certificate password.
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Validates that required options are present for the selected authentication mode.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            throw new InvalidOperationException("Dataverse URL is required.");
        }

        if (AuthMode == AuthenticationMode.ServicePrincipal)
        {
            if (string.IsNullOrWhiteSpace(TenantId))
            {
                throw new InvalidOperationException("Tenant ID is required for service principal authentication.");
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new InvalidOperationException("Client ID is required for service principal authentication.");
            }

            var hasSecret = !string.IsNullOrWhiteSpace(ClientSecret);
            var hasCertificate = !string.IsNullOrWhiteSpace(CertificatePath);

            if (!hasSecret && !hasCertificate)
            {
                throw new InvalidOperationException(
                    "Either client secret or certificate path is required for service principal authentication.");
            }
        }
    }
}
