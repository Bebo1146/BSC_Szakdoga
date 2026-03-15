using System.Security.Cryptography;
using System.Text;

namespace OAuthStart.OAuth2.AuthorizationCodeFlow
{
    internal class PkceGenerator
    {
        internal string CodeVerifier { get; }
        internal string CodeChallenge { get; }

        internal PkceGenerator()
        {
            CodeVerifier = GenerateCodeVerifier();
            CodeChallenge = GenerateCodeChallenge(CodeVerifier);
        }

        private static string GenerateCodeVerifier()
        {
            byte[] randomBytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Base64UrlEncode(randomBytes);
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}