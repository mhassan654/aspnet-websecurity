using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<User> userManager;

    public ConfirmEmailModel(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    [BindProperty] public string Message { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Message = "Email address is successfully confirmed, you can now try to login";

                return Page();
            }
        }

        Message = "Failed to validate email";
        return Page();
    }
}