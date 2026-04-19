using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using TokenValidation.TokenValidation;

namespace Tests
{
    [TestFixture]
    public class JwtClaimReaderTests
    {
        private static string CreateTestJwt(params (string type, string value)[] claims)
        {
            JwtSecurityTokenHandler handler = new();
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(claims.Select(c => new Claim(c.type, c.value))),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(new byte[32]),
                    SecurityAlgorithms.HmacSha256)
            };
            SecurityToken token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_BearerToken_ExtractsToken()
        {
            string? result = JwtClaimReader.GetTokenFromAuthorizationHeader("Bearer abc123");

            Assert.That(result, Is.EqualTo("abc123"));
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_BearerCaseInsensitive_ExtractsToken()
        {
            string? result = JwtClaimReader.GetTokenFromAuthorizationHeader("bearer mytoken");

            Assert.That(result, Is.EqualTo("mytoken"));
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_NullInput_ReturnsNull()
        {
            Assert.That(JwtClaimReader.GetTokenFromAuthorizationHeader(null), Is.Null);
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_EmptyString_ReturnsNull()
        {
            Assert.That(JwtClaimReader.GetTokenFromAuthorizationHeader(""), Is.Null);
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_WhitespaceOnly_ReturnsNull()
        {
            Assert.That(JwtClaimReader.GetTokenFromAuthorizationHeader("   "), Is.Null);
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_BearerWithEmptyToken_ReturnsNull()
        {
            Assert.That(JwtClaimReader.GetTokenFromAuthorizationHeader("Bearer "), Is.Null);
        }

        [Test]
        public void GetTokenFromAuthorizationHeader_NoBearerPrefix_ReturnsRawValue()
        {
            string? result = JwtClaimReader.GetTokenFromAuthorizationHeader("raw-token-value");

            Assert.That(result, Is.EqualTo("raw-token-value"));
        }

        [Test]
        public void GetTokenFromRequest_NullRequest_ReturnsNull()
        {
            Assert.That(JwtClaimReader.GetTokenFromRequest(null), Is.Null);
        }

        [Test]
        public void GetTokenFromRequest_NoAuthorizationHeader_ReturnsNull()
        {
            DefaultHttpContext context = new();

            Assert.That(JwtClaimReader.GetTokenFromRequest(context.Request), Is.Null);
        }

        [Test]
        public void GetTokenFromRequest_ValidBearerHeader_ReturnsTokenResponse()
        {
            DefaultHttpContext context = new();
            context.Request.Headers["Authorization"] = "Bearer test-token";

            TokenValidation.Jwt.TokenResponse? result = JwtClaimReader.GetTokenFromRequest(context.Request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AccessToken, Is.EqualTo("test-token"));
            Assert.That(result.TokenType, Is.EqualTo("Bearer"));
        }

        [Test]
        public void GetNameFromJwt_JwtWithNameClaim_ReturnsName()
        {
            string jwt = CreateTestJwt(("name", "John Doe"));
            TokenValidation.Jwt.TokenResponse token = new() { AccessToken = jwt };

            string? result = JwtClaimReader.GetNameFromJwt(token);

            Assert.That(result, Is.EqualTo("John Doe"));
        }

        [Test]
        public void GetNameFromJwt_JwtWithoutNameClaim_ReturnsNull()
        {
            string jwt = CreateTestJwt(("sub", "user-123"));
            TokenValidation.Jwt.TokenResponse token = new() { AccessToken = jwt };

            string? result = JwtClaimReader.GetNameFromJwt(token);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetPreferredNameFromJwt_JwtWithPreferredUsername_ReturnsIt()
        {
            string jwt = CreateTestJwt(("preferred_username", "johndoe"));
            TokenValidation.Jwt.TokenResponse token = new() { AccessToken = jwt };

            string? result = JwtClaimReader.GetPreferredNameFromJwt(token);

            Assert.That(result, Is.EqualTo("johndoe"));
        }

        [Test]
        public void GetPreferredNameFromJwt_JwtWithoutPreferredUsername_ReturnsNull()
        {
            string jwt = CreateTestJwt(("sub", "user-123"));
            TokenValidation.Jwt.TokenResponse token = new() { AccessToken = jwt };

            string? result = JwtClaimReader.GetPreferredNameFromJwt(token);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetNameFromJwt_UsesIdTokenWhenPresent()
        {
            string idTokenJwt = CreateTestJwt(("name", "From ID Token"));
            string accessTokenJwt = CreateTestJwt(("name", "From Access Token"));
            TokenValidation.Jwt.TokenResponse token = new()
            {
                AccessToken = accessTokenJwt,
                IdToken = idTokenJwt
            };

            string? result = JwtClaimReader.GetNameFromJwt(token);

            Assert.That(result, Is.EqualTo("From ID Token"));
        }

        [Test]
        public void GetNameFromJwt_FallsBackToAccessTokenWhenNoIdToken()
        {
            string accessTokenJwt = CreateTestJwt(("name", "From Access Token"));
            TokenValidation.Jwt.TokenResponse token = new()
            {
                AccessToken = accessTokenJwt,
                IdToken = null
            };

            string? result = JwtClaimReader.GetNameFromJwt(token);

            Assert.That(result, Is.EqualTo("From Access Token"));
        }
    }
}