using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

/* external refs note (see Notepad credit1)
   - logout clears cookie via HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
   - action is POST + [ValidateAntiForgeryToken] because state change
   - attribute route: [Route("Account")] + [HttpPost("LogoutRedirect")] (simple path /Account/LogoutRedirect)
   - after sign out redirect to Account/Login with RedirectToAction
   - refs: Microsoft Learn docs (cookie auth, SignOutAsync API, routing, anti-forgery)
   - understanding: cookie end flow + anti-forgery + local route mapping
*/


namespace EasyGames.Controllers
{
    // Dedicated endpoint to sign out and ALWAYS redirect to Login
    // It keeps it separate from any existing AccountController logic.
    [Route("Account")]
    public class AuthHelperController : Controller
    {
        [HttpPost("LogoutRedirect")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutRedirect()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Always go to Login after logout
            return RedirectToAction("Login", "Account");
        }
    }
}
