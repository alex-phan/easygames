using Microsoft.AspNetCore.Authorization;

namespace EasyGames.Filters
{
    // owner-only gate attribute; wraps [Authorize(Roles = "Owner")]
    // why: keep role string in one place, less copy/paste, controller stays clean
    // note: role comes from cookie claims (ClaimTypes.Role) after login
    public class AuthorizeOwnerAttribute : AuthorizeAttribute
    {
        public AuthorizeOwnerAttribute()
        {
            Roles = "Owner"; // only allow Owner role for this endpoint
        }
        // change here if role name changes later (single spot)
    }
}
