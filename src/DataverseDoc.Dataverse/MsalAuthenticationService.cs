using DataverseDoc.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace DataverseDoc.Dataverse;

/// <summary>
/// MSAL-based authentication service for Dataverse.
/// Supports interactive (device code) and service principal authentication.
/// </summary>
public class MsalAuthenticationService : IAuthenticationService
{
    private readonly ILogger<MsalAuthenticationService> _logger;
    private IPublicClientApplication? _publicClientApp;
    private IConfidentialClientApplication? _confidentialClientApp;

    public MsalAuthenticationService(ILogger<MsalAuthenticationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<string> GetAccessTokenAsync(DataverseConnectionOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        cancellationToken.ThrowIfCancellationRequested();

        options.Validate();
        ValidateOptions(options);

        var scope = GetScope(options.Url!);

        return options.AuthMode switch
        {
            AuthenticationMode.Interactive => await AcquireTokenInteractiveAsync(options, scope, cancellationToken),
            AuthenticationMode.ServicePrincipal => await AcquireTokenServicePrincipalAsync(options, scope, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported authentication mode: {options.AuthMode}")
        };
    }

    /// <summary>
    /// Gets the OAuth scope for the Dataverse environment.
    /// </summary>
    /// <param name="url">The Dataverse environment URL.</param>
    /// <returns>The OAuth scope.</returns>
    public string GetScope(string url)
    {
        var uri = new Uri(url.TrimEnd('/'));
        return $"{uri.Scheme}://{uri.Host}/.default";
    }

    private void ValidateOptions(DataverseConnectionOptions options)
    {
        if (options.AuthMode == AuthenticationMode.Interactive)
        {
            if (string.IsNullOrWhiteSpace(options.ClientId))
            {
                throw new InvalidOperationException(
                    "Client ID is required for interactive authentication. " +
                    "Register an application in Azure AD and provide the Application (client) ID.");
            }
        }
    }

    private async Task<string> AcquireTokenInteractiveAsync(
        DataverseConnectionOptions options,
        string scope,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting interactive authentication using device code flow");

        var authority = string.IsNullOrWhiteSpace(options.TenantId)
            ? "https://login.microsoftonline.com/common"
            : $"https://login.microsoftonline.com/{options.TenantId}";

        _publicClientApp = PublicClientApplicationBuilder
            .Create(options.ClientId)
            .WithAuthority(authority)
            .WithDefaultRedirectUri()
            .Build();

        var scopes = new[] { scope };

        try
        {
            // Try to acquire token silently first (from cache)
            var accounts = await _publicClientApp.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            if (account != null)
            {
                _logger.LogDebug("Found cached account, attempting silent authentication");
                try
                {
                    var silentResult = await _publicClientApp
                        .AcquireTokenSilent(scopes, account)
                        .ExecuteAsync(cancellationToken);

                    _logger.LogInformation("Token acquired silently from cache");
                    return silentResult.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    _logger.LogDebug("Silent authentication failed, falling back to device code flow");
                }
            }

            // Fall back to device code flow
            var result = await _publicClientApp
                .AcquireTokenWithDeviceCode(scopes, callback =>
                {
                    Console.WriteLine();
                    Console.WriteLine(callback.Message);
                    Console.WriteLine();
                    return Task.CompletedTask;
                })
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation("Token acquired via device code flow");
            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "MSAL authentication failed: {Error}", ex.Message);
            throw new InvalidOperationException(
                $"Interactive authentication failed: {ex.Message}. " +
                "Ensure you have the correct permissions and the application is configured properly.",
                ex);
        }
    }

    private async Task<string> AcquireTokenServicePrincipalAsync(
        DataverseConnectionOptions options,
        string scope,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting service principal authentication");

        var authority = $"https://login.microsoftonline.com/{options.TenantId}";
        var builder = ConfidentialClientApplicationBuilder
            .Create(options.ClientId)
            .WithAuthority(authority);

        if (!string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            _logger.LogDebug("Using client secret for authentication");
            builder = builder.WithClientSecret(options.ClientSecret);
        }
        else if (!string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            _logger.LogDebug("Using certificate for authentication");
            var certificate = LoadCertificate(options.CertificatePath, options.CertificatePassword);
            builder = builder.WithCertificate(certificate);
        }
        else
        {
            throw new InvalidOperationException(
                "Either client secret or certificate path is required for service principal authentication.");
        }

        _confidentialClientApp = builder.Build();

        var scopes = new[] { scope };

        try
        {
            var result = await _confidentialClientApp
                .AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation("Token acquired via service principal");
            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            _logger.LogError(ex, "MSAL service principal authentication failed: {Error}", ex.Message);
            throw new InvalidOperationException(
                $"Service principal authentication failed: {ex.Message}. " +
                "Verify the tenant ID, client ID, and credentials are correct.",
                ex);
        }
    }

    private X509Certificate2 LoadCertificate(string certificatePath, string? password)
    {
        _logger.LogDebug("Loading certificate from: {Path}", certificatePath);

        if (!File.Exists(certificatePath))
        {
            throw new FileNotFoundException(
                $"Certificate file not found: {certificatePath}",
                certificatePath);
        }

        try
        {
            X509Certificate2 certificate;

            if (string.IsNullOrWhiteSpace(password))
            {
                // Try loading as PEM/CER first, then as PKCS12
                certificate = X509CertificateLoader.LoadCertificateFromFile(certificatePath);
            }
            else
            {
                // Load as PKCS12/PFX with password
                certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, password);
            }

            _logger.LogDebug("Certificate loaded successfully. Subject: {Subject}", certificate.Subject);
            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate from: {Path}", certificatePath);
            throw new InvalidOperationException(
                $"Failed to load certificate from '{certificatePath}': {ex.Message}",
                ex);
        }
    }
}
