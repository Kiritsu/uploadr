﻿using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UploadR.Database;

namespace UploadR.Authentications
{
    public sealed class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public const string AuthenticationSchemeName = "TokenAuthenticationScheme";

        public const string ClaimToken = "https://schemas.alnmrc.com/claims/token";

        private readonly UploadRContext _database;
        private readonly ILogger<TokenAuthenticationHandler> _logger;

        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder, 
            ISystemClock clock, 
            UploadRContext database) : base(options, logger, encoder, clock)
        {
            _database = database;
            _logger = logger.CreateLogger<TokenAuthenticationHandler>();
        }

        /// <summary>
        ///     Tries to gather the token from the header 'Authentication'.
        /// </summary>
        /// <param name="token">Out value representing the token in the header.</param>
        private bool TryGetTokenByHeader(out string token)
        {
            token = null;

            if (!Request.Headers.TryGetValue("Authorization", out var values) || values.Count == 0)
            {
                return false;
            }

            token = values[0];
            return true;
        }
        
        /// <summary>
        ///     Tries to authenticates the user from the request.
        ///     This check if a token is passed in the headers. 
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!TryGetTokenByHeader(out var token) || token is null)
            {
                return AuthenticateResult.NoResult();
            }

            _logger.LogDebug("Parsed token: [{Token}]", token);
            
            var user = await _database.Users.FirstOrDefaultAsync(x => x.Token == token);
            if (user is null)
            {
                _logger.LogDebug("Invalid token");
                return AuthenticateResult.Fail("Invalid token");
            }

            if (user.Disabled)
            {
                _logger.LogDebug("Blocked user '{Guid}' attempted to log-in", user.Guid);
                return AuthenticateResult.Fail("User blocked");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Guid.ToString()),
                new Claim(ClaimTypes.Role, user.Type.ToString()),
                new Claim(ClaimToken, token!)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogDebug("User '{Guid}' just authenticated (Token: {Token})", user.Guid, token);
            return AuthenticateResult.Success(ticket);
        }
    }
}