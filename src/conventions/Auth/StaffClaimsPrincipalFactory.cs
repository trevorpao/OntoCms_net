using System.Security.Claims;
using OntoCms.Modules.Role;
using OntoCms.Modules.Staff;

namespace OntoCms.Conventions.Auth;

public sealed class StaffClaimsPrincipalFactory
{
    public ClaimsPrincipal CreatePrincipal(StaffFeed.AuthProfile profile, string authenticationType)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new(ClaimTypes.Name, profile.Account),
            new(ClaimTypes.Role, profile.RoleTitle),
            new("role_id", profile.RoleId.ToString()),
        };

        foreach (var authority in RoleFeed.ExpandAuthorityNames(profile.RolePriv))
        {
            claims.Add(new Claim(StaffAuthenticationHandler.AuthorityClaimType, authority));
        }

        var identity = new ClaimsIdentity(claims, authenticationType);
        return new ClaimsPrincipal(identity);
    }
}