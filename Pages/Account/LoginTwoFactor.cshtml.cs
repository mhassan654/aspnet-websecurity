using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;
        private readonly SignInManager<User> signInManager;
        
        [BindProperty]
        public EmailMFA EmailMFA { get; set; }

        public LoginTwoFactorModel(UserManager<User> userManager, IEmailService emailService, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.EmailMFA = new EmailMFA();
        }
        
        public async Task OnGetAsync(string email, bool rememberMe)
        {
            var user = await userManager.FindByEmailAsync(email);

            this.EmailMFA.SecurityCode = string.Empty;
            this.EmailMFA.RememberMe = rememberMe;
            
            // generate the code
            if (user != null)
            {
                var securityCode = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
            
                // send o the user
                await emailService.SendAsync("hassansaava@gmail.com",
                    email,
                    "My web app's OTP",
                    $"Please use this code as the OTP: {securityCode}");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var result = await signInManager.TwoFactorSignInAsync("Email",
                this.EmailMFA.SecurityCode,
                this.EmailMFA.RememberMe, 
                false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError("Login2FA", result.IsLockedOut ? "You are locked out." : "Failed to login.");
                return Page();
            }
        }
    }

    public class EmailMFA
    {
        [Required]
        [Display(Name = "Security Code")]
        public string SecurityCode { get; set; }
        public bool RememberMe { get; set; }
    }
}
