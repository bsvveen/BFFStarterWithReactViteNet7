
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("Login")]
        public IActionResult Login(string returnUrl) =>
            new ChallengeResult(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties {
                    RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
                });

        [AllowAnonymous]
        [HttpGet("LoginCallback")]
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("External");

            if (!authenticateResult.Succeeded)
                return BadRequest();

            var claimsIdentity = new ClaimsIdentity("Application");

            claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Name));
            claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.Email));

            await HttpContext.SignInAsync(
                "Application",
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }); // IsPersistent will set a cookie that lasts for two weeks (by default).            

            return LocalRedirect(returnUrl);
        }       
  
        [Authorize]
        [HttpGet("GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            var user = new UserInfo(HttpContext.User);
            return Ok(user);
        }

        private class UserInfo
        {
            private readonly ClaimsPrincipal _user;
            public UserInfo(ClaimsPrincipal user) => _user = user;

            public string Name => _user.FindFirst(ClaimTypes.Name).Value;
            public string Email => _user.FindFirst(ClaimTypes.Email).Value;
        }
    }
}
