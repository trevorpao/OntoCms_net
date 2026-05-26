using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OntoCms.Modules.Role;
using OntoCms.Modules.Staff;

namespace OntoCms.Conventions.Auth;

public sealed class StaffAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "StaffHeader";
    public const string CookieSchemeName = "StaffCookie";
    public const string CookieName = "onto_staff";
    public const string HeaderName = "X-Onto-Staff-Id";
    public const string AuthorityClaimType = "authority";

    private readonly StaffClaimsPrincipalFactory claimsPrincipalFactory;
    private readonly StaffFeed staffFeed;

    public StaffAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        StaffClaimsPrincipalFactory claimsPrincipalFactory,
        StaffFeed staffFeed)
        : base(options, logger, encoder)
    {
        this.claimsPrincipalFactory = claimsPrincipalFactory;
        this.staffFeed = staffFeed;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        if (!int.TryParse(headerValues.FirstOrDefault(), out var staffId) || staffId <= 0)
        {
            return AuthenticateResult.Fail($"{HeaderName} must be a positive integer.");
        }

        var profile = await staffFeed.GetAuthProfileAsync(staffId, Context.RequestAborted);
        if (profile is null)
        {
            return AuthenticateResult.Fail("Staff not found.");
        }

        if (!string.Equals(profile.Status, "Verified", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Staff is not in a verified status.");
        }

        if (!string.Equals(profile.RoleStatus, "Enabled", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Role is not enabled.");
        }

        var principal = claimsPrincipalFactory.CreatePrincipal(profile, SchemeName);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}