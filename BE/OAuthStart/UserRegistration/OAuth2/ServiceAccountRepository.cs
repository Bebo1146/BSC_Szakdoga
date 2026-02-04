using System.Text.Json;

namespace UserManagement.OAuth2
{
    public class ServiceAccountRepository
    {
        private readonly Uri _address;

        public ServiceAccountRepository(Uri address)
        {
            _address = address;
        }

        public async Task<JsonDocument> GetAsync(string clientId, string token) 
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string userInfoJson = await client.GetStringAsync(new Uri($"{_address}/clients/{clientId}/service-account-user"));

                return JsonDocument.Parse(userInfoJson);
            }
        }
    }
}