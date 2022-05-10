namespace BOTS.Web.Extensions
{
    using System.Security.Claims;

    public static class UserClaimsExtensions
    {
        public static string GetUserId(this ClaimsPrincipal userClaims)
        {
            return userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
