using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TestExamIS.Tests.Utils;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string UserType { get; set; } = "user";

    public static class TestUsers
    {
        public static readonly string UserId = "test-user-1";
        public static readonly string AdminId = "test-admin-id";
        public static readonly string UserEmail = "liam.smith0@university.edu";
        public static readonly string AdminEmail = "admin@university.edu";
        public static readonly string UserRole = "Professor";
        public static readonly string AdminRole = "Professor";
    }

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Test-User"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        Claim[] claims;

        if (UserType.ToLower() == "admin")
        {
            claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUsers.AdminId),
                new Claim(ClaimTypes.Name, TestUsers.AdminEmail),
                new Claim(ClaimTypes.Email, TestUsers.AdminEmail),
                new Claim(ClaimTypes.Role, TestUsers.AdminRole)
            };
        }
        else
        {
            claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUsers.UserId),
                new Claim(ClaimTypes.Name, TestUsers.UserEmail),
                new Claim(ClaimTypes.Email, TestUsers.UserEmail),
                new Claim(ClaimTypes.Role, TestUsers.UserRole)
            };
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
