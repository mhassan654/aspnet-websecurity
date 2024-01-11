using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using WebApp.Data.Account;

namespace WebApp.Pages.Account;

[Authorize]
public class AuthenticatorWithMfaSetupModel : PageModel
{
    private readonly UserManager<User> userManager;

    public AuthenticatorWithMfaSetupModel(UserManager<User> userManager)
    {
        this.userManager = userManager;
        ViewModel = new SetupMfaViewModel();
        Succeeded = false;
    }

    [BindProperty] public SetupMfaViewModel ViewModel { get; set; }

    [BindProperty] public bool Succeeded { get; set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user != null)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            var key = await userManager.GetAuthenticatorKeyAsync(user);
            ViewModel.Key = key ?? string.Empty;
            ViewModel.QRCodeBytes = GenerateQRCodeBytes("my web app",
                ViewModel.Key,
                user.Email ?? string.Empty);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await userManager.GetUserAsync(User);
        if (user != null && await userManager.VerifyTwoFactorTokenAsync(
                user, userManager.Options.Tokens.AuthenticatorTokenProvider, ViewModel.SecurityCode))
        {
            await userManager.SetTwoFactorEnabledAsync(user, true);
            Succeeded = true;
        }
        else
        {
            ModelState.AddModelError("AuthenticatorSetup", "Something went wrong with the authenticator setup");
        }

        return Page();
    }

    private byte[] GenerateQRCodeBytes(string provider, string key, string userEmail)
    {
        var qrCodeGenerator = new QRCodeGenerator();
        var qrCodeData = qrCodeGenerator.CreateQrCode(
            $"otpauth://totp/{provider}:{userEmail}?secret={key}&issuer={provider}",
            QRCodeGenerator.ECCLevel.Q
        );

        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public class SetupMfaViewModel
    {
        public string Key { get; set; }

        [Required] [Display(Name = "Code")] public string SecurityCode { get; set; } = string.Empty;

        public byte[]? QRCodeBytes { get; set; }
    }
}