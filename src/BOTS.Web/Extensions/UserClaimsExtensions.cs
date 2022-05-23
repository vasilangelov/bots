namespace BOTS.Web.Extensions
{
    using System.Security.Claims;

    public static class UserClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal userClaims)
        {
            return Guid.Parse(userClaims.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
