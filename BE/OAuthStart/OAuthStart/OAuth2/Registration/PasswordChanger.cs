using System.Text;

namespace OAuthStart.OAuth2.Registration
{
    internal class PasswordChanger
    {
        internal async Task ChangePasswordAsync(Uri uri, string newPassword, string token, CancellationToken cancellationToken = default)
        {
            using (HttpClient client = new HttpClient())
            {
                string passwordChangeBody = $$"""
                {
                    "type": "password",
                    "value": "{{newPassword}}",
                    "temporary": false
                }
                """;

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                StringContent content = new StringContent(passwordChangeBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(uri, content, cancellationToken);

                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                response.EnsureSuccessStatusCode();

                Console.WriteLine("Password changed successfully");
            }
        }
    }
}