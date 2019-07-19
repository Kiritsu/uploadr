using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareY.Database;

namespace ShareY.Authentications
{
    public sealed class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public const string AuthenticationSchemeName = "TokenAuthenticationScheme";

        private readonly ShareYContext _database;
        private readonly ILogger<TokenAuthenticationHandler> _logger;

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, 
            UrlEncoder encoder, ISystemClock clock, ShareYContext database) : base(options, logger, encoder, clock)
        {
            _database = database;
            _logger = logger.CreateLogger<TokenAuthenticationHandler>();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Token", out var values) || values.Count == 0)
            {
                _logger.LogWarning("Missing Authorization.");
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization."));
            }

            var token = values.First();
            if (!Guid.TryParse(token, out var tokenGuid))
            {
                _logger.LogWarning("Missing Authorization.");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Token Format."));
            }

            var validatedToken = _database.Tokens.FirstOrDefault(x => x.Guid == tokenGuid);
            if (validatedToken is null)
            {
                _logger.LogWarning("Invalid Token.");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Token."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, validatedToken.UserGuid.ToString()),
                new Claim(ClaimTypes.Authentication, validatedToken.Guid.ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("User '{0}' just authenticated. (Token: {1})", validatedToken.UserGuid, validatedToken.Guid);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
