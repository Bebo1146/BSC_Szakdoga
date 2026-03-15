using System.Security.Cryptography;
using System.Text;

namespace OAuthCodeFlowService.Services
{
    public class PkceService : IPkceService
    {
        public string GenerateCodeVerifier()
        {
            byte[] randomBytes = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Base64UrlEncode(randomBytes);
        }

        public string GenerateCodeChallenge(string codeVerifier)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(challengeBytes);
        }

        public string GenerateState()
        {
            byte[] randomBytes = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Base64UrlEncode(randomBytes);
        }

        private static string Base64UrlEncode(byte[] input) =>
            Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}