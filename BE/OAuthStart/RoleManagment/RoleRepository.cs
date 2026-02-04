using System.Text;
using System.Text.Json;

namespace RoleManagment
{
    public class RoleRepository
    {
        private readonly Uri _address;
        private readonly string _realmManagementClientId;

        public RoleRepository(Uri address, string realmManagementClientId)
        {
            _address = address;
            _realmManagementClientId = realmManagementClientId;
        }

        public async Task<JsonDocument> GetAsync(string roleName, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string role = await client.GetStringAsync(new Uri($"{_address}clients/{_realmManagementClientId}/roles/{roleName}"));

                return JsonDocument.Parse(role);
            }
        }

        public async Task<JsonDocument> GetUserRolesAsync(string userId, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string rolesAssignedToUser = await client.GetStringAsync(new Uri($"{_address}users/{userId}/role-mappings/clients/{_realmManagementClientId}"));

                return JsonDocument.Parse(rolesAssignedToUser);
            }
        }

        public async Task AsignRoleToUser(string userId, JsonDocument role, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string roleId = role.RootElement.GetProperty("id").GetString();
                string roleName = role.RootElement.GetProperty("name").GetString();

                string roleToAssign = $$"""
                [
                  {
                    "id": "{{roleId}}",
                    "name": "{{roleName}}"
                  }
                ]
                """;

                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_address}users/{userId}/role-mappings/clients/{_realmManagementClientId}")
                {
                    Content = new StringContent(roleToAssign, Encoding.UTF8, "application/json")
                };

                await client.SendAsync(req);
            }
        }
    }
}
