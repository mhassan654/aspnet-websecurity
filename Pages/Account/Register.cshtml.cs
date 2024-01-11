using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IEmailService emailService;
    private readonly UserManager<User> userManager;

    public RegisterModel(UserManager<User> userManager, IEmailService emailService)
    {
        this.userManager = userManager;
        this.emailService = emailService;
    }

    [BindProperty] public RegisterViewModel RegisterViewModel { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        {
            // validate email adress (optional)

            // create the user
            var user = new User
            {
                Email = RegisterViewModel.Email,
                UserName = RegisterViewModel.Email,
                Department = RegisterViewModel.Department,
                Position = RegisterViewModel.Position
            };

            var claimDepartment = new Claim("Department", RegisterViewModel.Department);
            var claimPosition = new Claim("Position", RegisterViewModel.Position);

            var result = await userManager.CreateAsync(user, RegisterViewModel.Password);
            if (result.Succeeded)
            {
                await userManager.AddClaimAsync(user, claimDepartment);
                await userManager.AddClaimAsync(user, claimPosition);

                var comnfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.PageLink("/Account/ConfirmEmail",
                    values: new { userId = user.Id, token = comnfirmationToken }) ?? "";

                await emailService.SendAsync("hassansaava@gmail.com",
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");


                return RedirectToPage("/Account/Login");
            }

            foreach (var error in result.Errors) ModelState.AddModelError("Register", error.Description);

            return Page();
        }
    }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email adress")]
    public string Email { get; set; } = string.Empty;


    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required] public string Department { get; set; } = string.Empty;

    [Required] public string Position { get; set; } = string.Empty;
}