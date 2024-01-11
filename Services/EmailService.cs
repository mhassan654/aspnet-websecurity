using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using WebApp.Settings;

namespace WebApp.Services;

public class EmailService : IEmailService
{
    private readonly IOptions<SmtpSettings> smtpSetting;

    public EmailService(IOptions<SmtpSettings> smtpSetting)
    {
        this.smtpSetting = smtpSetting;
    }

    public async Task SendAsync(string from, string to, string subject, string body)
    {
        var message = new MailMessage(from, to, subject, body);

        using (
            var client = new SmtpClient(
                smtpSetting.Value.Host, int.Parse(smtpSetting.Value.Port)))
        {
            client.Credentials = new NetworkCredential(
                smtpSetting.Value.User, smtpSetting.Value.Password);

            await client.SendMailAsync(message);
        }

        ;
    }
}