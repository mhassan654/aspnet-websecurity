using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Data.Account;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;

    public AccountController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    // GET
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo != null)
        {
            var emailClaims = loginInfo.Principal.Claims.FirstOrDefault(
                x => x.Type == ClaimTypes.Email);
            var userClaims = loginInfo.Principal.Claims.FirstOrDefault(
                x => x.Type == ClaimTypes.Name);

            if (emailClaims != null && userClaims != null)
            {
                var user = new User { Email = emailClaims.Value, UserName = userClaims.Value };
                await _signInManager.SignInAsync(user, false);
            }
        }

        return RedirectToPage("/Index");
    }
}