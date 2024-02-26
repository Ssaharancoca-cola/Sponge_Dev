namespace Sponge.Common;

using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly SPONGE_Context _dbContext;

    public CustomClaimsTransformation(SPONGE_Context dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = (ClaimsIdentity)principal.Identity;

        // Extract all claims except the role claims
        var newClaims = claimsIdentity.Claims
                                      .Where(c => c.Type != ClaimTypes.Role)
                                      .ToList();

        // Get the username from the principal (example assumes DOMAIN\username format)
        var userNameSplit = principal.Identity.Name?.Split('\\');
        var userName = userNameSplit?.Length > 1 ? userNameSplit[1] : null;

        if (userName == null)
        {
            throw new Exception("User name cannot be determined from the principal.");
        }

        // Retrieve roles from the database asynchronously
        var userRoles = await (from user in _dbContext.SPG_USERS
                               join userFunc in _dbContext.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
                               join role in _dbContext.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
                               where user.USER_ID == userName
                               orderby role.ROLE_ID descending
                               select role.ROLE_NAME).ToListAsync();

        // Add the new role claims
        newClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Create a new ClaimsIdentity with the updated claims
        var newIdentity = new ClaimsIdentity(newClaims, claimsIdentity.AuthenticationType);

        // Create a new ClaimsPrincipal with the new identity
        var newPrincipal = new ClaimsPrincipal(newIdentity);

        return newPrincipal;
    }

}
