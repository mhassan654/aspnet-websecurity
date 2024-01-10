using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> sigInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            this.sigInManager = signInManager;
        }

        [BindProperty]
        public CredentialViewModel Credential { get; set; } = new CredentialViewModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await sigInManager.PasswordSignInAsync(
                this.Credential.Email,
                this.Credential.Password,
                this.Credential.RememberMe,
                false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if(result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginTwoFAWithAuthenticator", 
                        new {this.Credential.RememberMe
                    });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login.");
                }

                return Page();
            }
        }
               
    }

    public class CredentialViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;


        [Required]
        [DataType(dataType: DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }    
    }
}
