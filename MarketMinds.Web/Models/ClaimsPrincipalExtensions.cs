using System.Security.Claims;

namespace MarketMinds.Web.Models
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        /// <param name="principal">The claims principal</param>
        /// <param name="defaultId">The default ID to return if the user is not authenticated or ID cannot be parsed</param>
        /// <returns>The user's ID if authenticated and ID exists, otherwise the defaultId</returns>
        public static int GetCurrentUserId(this ClaimsPrincipal principal, int defaultId = 1)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return defaultId;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return defaultId;
        }
    }
}