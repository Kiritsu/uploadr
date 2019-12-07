using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UploadR.Database;
using UploadR.Extensions;

namespace UploadR.Authentications
{
    public sealed class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public const string AuthenticationSchemeName = "TokenAuthenticationScheme";

        public const string ClaimToken = "https://schemas.alnmrc.com/claims/token";

        private readonly UploadRContext _database;
        private readonly ILogger<TokenAuthenticationHandler> _logger;

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, UploadRContext database) : base(options, logger, encoder, clock)
        {
            _database = database;
            _logger = logger.CreateLogger<TokenAuthenticationHandler>();
        }

        private bool TryGetTokenByHeader(out Guid? token)
        {
            token = null;

            if (!Request.Headers.TryGetValue("X-Token", out var values) || values.Count == 0)
            {
                return false;
            }

            var tokenStr = values.First();
            if (!Guid.TryParse(tokenStr, out var tokenPrs))
            {
                return false;
            }

            token = tokenPrs;
            return true;
        }

        private bool TryGetTokenFromSession(out Guid? token)
        {
            token = null;

            if (!Request.HttpContext.Session.IsAvailable)
            {
                return false;
            }

            token = Request.HttpContext.Session.Get<Guid?>("userToken");
            return token != null;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!TryGetTokenByHeader(out var tokenGuid) && !TryGetTokenFromSession(out tokenGuid))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization."));
            }

            var validatedToken = _database.Tokens.Include(x => x.User).FirstOrDefault(x => x.Guid == tokenGuid);
            if (validatedToken is null)
            {
                _logger.LogWarning("Invalid Token.");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Token."));
            }

            if (validatedToken.Revoked)
            {
                _logger.LogWarning("Token revoked.");
                return Task.FromResult(AuthenticateResult.Fail("Token revoked."));
            }

            if (validatedToken.User.Disabled)
            {
                _logger.LogWarning("User blocked.");
                return Task.FromResult(AuthenticateResult.Fail("User blocked."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, validatedToken.User.Email),
                new Claim(ClaimTypes.NameIdentifier, validatedToken.UserGuid.ToString()),
                new Claim(ClaimTypes.Email, validatedToken.User.Email),
                new Claim(ClaimToken, validatedToken.Guid.ToString()),
                new Claim(ClaimTypes.Role, validatedToken.TokenType.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("User '{0}' just authenticated. (Token: {1})", validatedToken.UserGuid, validatedToken.Guid);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
