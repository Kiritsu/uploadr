using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using ShareY.Configurations;
using ShareY.Database;
using ShareY.Interfaces;

namespace ShareY.Services
{
    public sealed class EmailService
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly SmtpClient _smtpClient;
        private readonly ShareYContext _dbContext;

        public EmailService(IEmailConfigurationProvider emailConfig, SmtpClient smtpClient, ShareYContext dbContext)
        {
            _emailConfiguration = emailConfig.GetConfiguration();
            _smtpClient = smtpClient;
            _dbContext = dbContext;
        }

        public async Task SendMagickUrlAsync(OneTimeToken ott, string baseUrl)
        {
            var user = await _dbContext.Users.FindAsync(ott.UserGuid);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailConfiguration.Auth));
            email.To.Add(new MailboxAddress(user.Email));
            email.Subject = "A Magick-Url has been requested on your account.";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>Hello! A magick-url has been requested on your account. Click the following url to log-in: <a href=\"{baseUrl}/auth/login/ott/{ott.Token}\">{baseUrl}/auth/login/ott/{ott.Token}</a></p><p>If you believe this is an error, you can ignore this.</p>"
            };

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_emailConfiguration.Server, _emailConfiguration.Port, _emailConfiguration.UseSsl);
            }

            if (!_smtpClient.IsAuthenticated)
            {
                await _smtpClient.AuthenticateAsync(_emailConfiguration.Auth, _emailConfiguration.Password);
            }

            await _smtpClient.SendAsync(email);
        }
    }
}
