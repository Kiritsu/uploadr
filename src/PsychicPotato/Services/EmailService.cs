﻿using System;
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

            await SendEmailAsync(user, "A magick-url has been requested on your account.",
                $"<p>Hello! A magick-url has been requested on your account. " +
                $"Click the following url to log-in: <a href=\"{baseUrl}/auth/login/ott/{ott.Token}\">{baseUrl}/auth/login/ott/{ott.Token}</a></p>" +
                $"<p>If you believe this is an error, you can ignore this.</p>");
        }

        public async Task SendSignupSuccessAsync(string token, string baseUrl)
        {
            var user = await _dbContext.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Token.Guid.ToString() == token);

            await SendEmailAsync(user, "Your account has been created.",
                $"<p>Hello! Thank you for registering an account for our service. Your token is: {user.Token.Guid}</p>" +
                $"<p>If you believe this is an error, please reach us on <a href=\"{baseUrl}\">{baseUrl}</a>.</p>");
        }

        public Task SendCustomActionAsync(User user, string action)
        {
            return SendEmailAsync(user, "Your account has been updated",
                $"<p>Hello! This mail is just to inform you that the following action has been taken on your account: {action}. " +
                $"If it was not intended, contact us.</p>");
        }

        public async Task SendEmailAsync(User user, string subject, string content)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("User don't have any valid email.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailConfiguration.Auth));
            email.To.Add(new MailboxAddress(user.Email));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>{content}</p>"
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
