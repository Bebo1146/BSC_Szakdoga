using System.Net;
using System.Text.Json;

namespace UserManagement.Repository
{
    internal class UserRepository
    {
        private readonly Uri _baseAddress;

        public UserRepository(Uri baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public async Task<JsonDocument> GetAsync(string username, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string userInfoJson = await client.GetStringAsync(new Uri($"{_baseAddress}users?username={username}"));

                return JsonDocument.Parse(userInfoJson);
            }
        }
    }
}
