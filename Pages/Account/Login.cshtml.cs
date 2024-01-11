using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> signInManager;

    public LoginModel(SignInManager<User> signInManager)
    {
        this.signInManager = signInManager;
    }

    [BindProperty] public CredentialViewModel Credential { get; set; } = new();


    [BindProperty] public IEnumerable<AuthenticationScheme> ExternalLoginProviders { get; set; }

    public async Task OnGetAsync()
    {
        ExternalLoginProviders = await signInManager.GetExternalAuthenticationSchemesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(
            Credential.Email,
            Credential.Password,
            Credential.RememberMe,
            false);

        if (result.Succeeded)
        {
            return RedirectToPage("/Index");
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("/Account/LoginTwoFAWithAuthenticator",
                new
                {
                    Credential.RememberMe
                });
        if (result.IsLockedOut)
            ModelState.AddModelError("Login", "You are locked out.");
        else
            ModelState.AddModelError("Login", "Failed to login.");

        return Page();
    }

    public IActionResult OnPostLoginExternally(string provider)
    {
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, null);
        properties.RedirectUri = Url.Action("ExternalLoginCallback", "Account");

        return Challenge(properties, provider);
    }
}

public class CredentialViewModel
{
    [Required] public string Email { get; set; } = string.Empty;


    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")] public bool RememberMe { get; set; }
}