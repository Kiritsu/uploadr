using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using PsychicPotato.Configurations;
using PsychicPotato.Database;
using PsychicPotato.Database.Models;
using PsychicPotato.Interfaces;

namespace PsychicPotato.Services
{
    public sealed class EmailService
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly SmtpClient _smtpClient;
        private readonly PsychicPotatoContext _dbContext;

        public EmailService(IEmailConfigurationProvider emailConfig, SmtpClient smtpClient, PsychicPotatoContext dbContext)
        {
            _emailConfiguration = emailConfig.GetConfiguration();
            _smtpClient = smtpClient;
            _dbContext = dbContext;

            smtpClient.Timeout = _emailConfiguration.Timeout * 1000;
            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        public async Task SendMagickUrlAsync(OneTimeToken ott, string baseUrl)
        {
            var user = await _dbContext.Users.FindAsync(ott.UserGuid);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailConfiguration.Sender));
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

            if (!_smtpClient.IsAuthenticated && !string.IsNullOrWhiteSpace(_emailConfiguration.Auth))
            {
                await _smtpClient.AuthenticateAsync(_emailConfiguration.Auth, _emailConfiguration.Password);
            }

            await _smtpClient.SendAsync(email);
        }

        public async Task SendSignupSuccessAsync(string token, string baseUrl)
        {
            var user = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Token.Guid.ToString() == token);

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailConfiguration.Auth));
            email.To.Add(new MailboxAddress(user.Email));
            email.Subject = "Your account has been created.";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>Hello! Thank you for registering an account for our service. Your token is: {user.Token.Guid}</p><p>If you believe this is an error, please reach us on <a href=\"{baseUrl}\">{baseUrl}</a>.</p>"
            };

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_emailConfiguration.Server, _emailConfiguration.Port, _emailConfiguration.UseSsl);
            }

            if (!_smtpClient.IsAuthenticated && !string.IsNullOrWhiteSpace(_emailConfiguration.Auth))
            {
                await _smtpClient.AuthenticateAsync(_emailConfiguration.Auth, _emailConfiguration.Password);
            }

            await _smtpClient.SendAsync(email);
        }

        public async Task SendCustomActionAsync(User user, string action)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailConfiguration.Auth));
            email.To.Add(new MailboxAddress(user.Email));
            email.Subject = "Your account has been updated.";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>Hello! This mail is just to inform you that the following action has been taken on your account: {action}. If it was not intended, contact us.</p>"
            };

            if (!_smtpClient.IsConnected)
            {
                await _smtpClient.ConnectAsync(_emailConfiguration.Server, _emailConfiguration.Port, _emailConfiguration.UseSsl);
            }

            if (!_smtpClient.IsAuthenticated && !string.IsNullOrWhiteSpace(_emailConfiguration.Auth))
            {
                await _smtpClient.AuthenticateAsync(_emailConfiguration.Auth, _emailConfiguration.Password);
            }

            await _smtpClient.SendAsync(email);
        }
    }
}
