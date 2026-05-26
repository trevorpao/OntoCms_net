using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OntoCms.Conventions.Auth;
using OntoCms.Conventions.HMVC;

namespace OntoCms.Modules.Staff;

[ApiController]
[Route("api/staff")]
public sealed class StaffReaction : BaseReactionController
{
    private readonly StaffClaimsPrincipalFactory claimsPrincipalFactory;
    private readonly StaffFeed staffFeed;

    public StaffReaction(StaffClaimsPrincipalFactory claimsPrincipalFactory, StaffFeed staffFeed)
    {
        this.claimsPrincipalFactory = claimsPrincipalFactory;
        this.staffFeed = staffFeed;
    }

    [AllowAnonymous]
    [AcceptVerbs("GET", "HEAD", "POST")]
    [Route("{method}")]
    public Task<IActionResult> Reroute(
        string method,
        [FromForm] LoginRequest request,
        [FromQuery] string? loginredirect,
        CancellationToken cancellationToken)
    {
        return method.Trim().ToLowerInvariant() switch
        {
            "login" => Login(request, loginredirect, cancellationToken),
            "logout" => Logout(cancellationToken),
            "status" => Status(cancellationToken),
            "session" => Session(cancellationToken),
            _ => Task.FromResult<IActionResult>(NotFound()),
        };
    }

    private async Task<IActionResult> Login(
        LoginRequest request,
        string? loginredirect,
        CancellationToken cancellationToken)
    {
        return await ExecuteReactionAsync(async token =>
        {
            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Account))
            {
                missingFields.Add(nameof(request.Account));
            }

            if (string.IsNullOrWhiteSpace(request.Pwd))
            {
                missingFields.Add(nameof(request.Pwd));
            }

            if (missingFields.Count > 0)
            {
                return OkMissingData(missingFields);
            }

            var account = request.Account!.Trim();
            var password = request.Pwd!;

            var profile = await staffFeed.GetAuthProfileByAccountAsync(account, token);
            if (profile is null || string.IsNullOrWhiteSpace(profile.PasswordHash) || !VerifyPassword(password, profile.PasswordHash))
            {
                return OkWrongData(new { fields = new[] { "Account", "Pwd" } });
            }

            if (!string.Equals(profile.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                return OkUnverified(new { fields = new[] { "Status" } });
            }

            if (!string.Equals(profile.RoleStatus, "Enabled", StringComparison.OrdinalIgnoreCase))
            {
                return OkUnverified(new { fields = new[] { "Role" } });
            }

            var principal = claimsPrincipalFactory.CreatePrincipal(profile, StaffAuthenticationHandler.CookieSchemeName);
            await HttpContext.SignInAsync(StaffAuthenticationHandler.CookieSchemeName, principal);

            return OkDone(new
            {
                redirect = NormalizeRedirect(loginredirect),
                id = profile.Id,
                account = profile.Account,
            });
        }, cancellationToken);
    }

    private async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        return await ExecuteReactionAsync(async _ =>
        {
            await HttpContext.SignOutAsync(StaffAuthenticationHandler.CookieSchemeName);
            return OkDone(new { isLogin = false });
        }, cancellationToken);
    }

    private async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        return await ExecuteReactionAsync(async _ =>
        {
            var principal = await GetAuthenticatedPrincipalAsync();
            if (principal is null)
            {
                return OkDone(new { isLogin = 0 });
            }

            return OkDone(new
            {
                isLogin = 1,
                user = CreateUserPayload(principal),
            });
        }, cancellationToken);
    }

    private async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        return await ExecuteReactionAsync(async _ =>
        {
            var principal = await GetAuthenticatedPrincipalAsync();
            if (principal is null)
            {
                return Unauthorized();
            }

            return OkDone(CreateUserPayload(principal));
        }, cancellationToken);
    }

    private async Task<ClaimsPrincipal?> GetAuthenticatedPrincipalAsync()
    {
        var authResult = await HttpContext.AuthenticateAsync(StaffAuthenticationHandler.CookieSchemeName);
        return authResult.Succeeded ? authResult.Principal : null;
    }

    private static object CreateUserPayload(ClaimsPrincipal principal)
    {
        return new
        {
            id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
            account = principal.FindFirstValue(ClaimTypes.Name),
            role = principal.FindFirstValue(ClaimTypes.Role),
            role_id = principal.FindFirstValue("role_id"),
            authorities = principal.FindAll(StaffAuthenticationHandler.AuthorityClaimType)
                .Select(static claim => claim.Value)
                .ToArray(),
        };
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var normalizedHash = passwordHash.StartsWith("$2y$", StringComparison.Ordinal)
            ? string.Concat("$2a$", passwordHash.AsSpan(4))
            : passwordHash;

        return BCrypt.Net.BCrypt.Verify(password, normalizedHash);
    }

    private static string NormalizeRedirect(string? loginredirect)
    {
        if (string.IsNullOrWhiteSpace(loginredirect))
        {
            return "/backend/";
        }

        return loginredirect.StartsWith("/", StringComparison.Ordinal)
            ? loginredirect
            : "/backend/";
    }

    public sealed record LoginRequest(string? Account, string? Pwd);
}