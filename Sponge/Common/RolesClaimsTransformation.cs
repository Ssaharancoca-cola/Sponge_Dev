// A simple example of a claims transformation middleware
using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class RolesClaimsTransformation : IClaimsTransformation
{
    private readonly SPONGE_Context _dbContext;

    public RolesClaimsTransformation(SPONGE_Context dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;
        var userName = principal.Identity.Name.Split(new[] { "\\" }, StringSplitOptions.None);
        var userRoles = (from user in _dbContext.SPG_USERS
                           join userFunc in _dbContext.SPG_USERS_FUNCTION on user.USER_ID equals userFunc.USER_ID
                           join role in _dbContext.SPG_ROLE on userFunc.ROLE_ID equals role.ROLE_ID
                           where user.USER_ID == userName[1]
                           orderby role.ROLE_ID descending
                           select new
                           {
                               NAME = user.Name,
                               ROLE = role.ROLE_NAME
                           }).FirstOrDefault();
        // Add role claims to the identity
       
            identity.AddClaim(new Claim(ClaimTypes.Role, userRoles.ROLE));

        return principal;
    }
}
