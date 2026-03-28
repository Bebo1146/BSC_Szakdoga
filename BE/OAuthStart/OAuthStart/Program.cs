using System.Data;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OAuthStart.OAuth2.FlowClients;
using OAuthStart.OAuth2.Registration;
using OAuthStart.OAuth2HttpCommunication;
using OAuthStart.OAuth2HttpCommunication.FlowClients;
using RoleManagment;
using UserManagement.OAuth2;

namespace OAuthStart
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string devRealmIssuer = "http://localhost:8080/realms/dev-realm/";
            string masterIssuer = "http://localhost:8080/realms/master/";

            DiscoveryClient devRealmDiscoveryClient = new DiscoveryClient(devRealmIssuer);
            DiscoveryClient masterDiscoveryClient = new DiscoveryClient(masterIssuer);

            OpenIdConnectConfiguration masterDiscoveryDocument = masterDiscoveryClient.GetDiscoveryDocumentAsync().Result;
            OpenIdConnectConfiguration devDiscoveryDocument = devRealmDiscoveryClient.GetDiscoveryDocumentAsync().Result;

            Console.WriteLine(masterDiscoveryDocument.TokenEndpoint);

            ResourceOwnerPasswordCredentialsFlowClient ROPCFlowClient =
                new ResourceOwnerPasswordCredentialsFlowClient(new Uri(masterDiscoveryDocument.TokenEndpoint), "admin-cli", "admin", "admin");

            string token = await ROPCFlowClient.GetTokenAsync();

            Console.WriteLine(token);

            string clientCredJson = """
                {
                  "clientId": "svc-client",
                  "enabled": true,
                  "protocol": "openid-connect",
                  "publicClient": false,
                  "serviceAccountsEnabled": true,
                  "standardFlowEnabled": false,
                  "directAccessGrantsEnabled": false
                }
                """;

            string ropcJson = """
                {
                    "clientId": "ropc-client",
                    "enabled": true,
                    "protocol": "openid-connect",
                    "publicClient": true,
                    "standardFlowEnabled": false,
                    "directAccessGrantsEnabled": true,
                    "serviceAccountsEnabled": false
                }
                """;

            string authCodeFlowJson = """
                {
                    "clientId": "postman-code-client",
                    "enabled": true,
                    "protocol": "openid-connect",
                    "publicClient": false,
                    "standardFlowEnabled": true,
                    "directAccessGrantsEnabled": false,
                    "serviceAccountsEnabled": false,
                    "redirectUris": ["https://oauth.pstmn.io/v1/callback"]
                }
                """;


            string myBackendClient = """
            {
              "clientId": "my-backend-client6",
              "name": "Backend OAuth Code Flow Client",
              "enabled": true,
              "protocol": "openid-connect",
              "publicClient": false,
              "standardFlowEnabled": true,
              "directAccessGrantsEnabled": false,
              "serviceAccountsEnabled": false,
              "redirectUris": [
                "https://localhost:7037/api/auth/callback",
                "http://localhost:5215/api/auth/callback",
                "http://localhost:4200/api/auth/callback",
                "http://localhost:4200/auth-callback",
                "https://oauth.pstmn.io/v1/callback"
              ],
              "webOrigins": [
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200"
              ],
              "attributes": {
                "pkce.code.challenge.method": "S256",
                "post.logout.redirect.uris": "http://localhost:4200"
              }
            }
            """;

            JsonDocument myBackendClientToRegister = JsonDocument.Parse(
    myBackendClient,
    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

            //{
            //    "clientId": "my-backend-client",
            //  "enabled": true,
            //  "protocol": "openid-connect",
            //  "publicClient": false,
            //  "standardFlowEnabled": true,
            //  "directAccessGrantsEnabled": false,
            //  "serviceAccountsEnabled": false,
            //  "redirectUris": [
            //    "https://api.example.com/auth/callback"
            //  ]
            //}

            JsonDocument clientCredToRegister = JsonDocument.Parse(clientCredJson);

            ClientRegister clientRegister = new ClientRegister();

            await clientRegister.RegisterAsync(myBackendClientToRegister, new Uri("http://localhost:8080/admin/realms/dev-realm/clients"), token);

            //try
            //{
            //    await clientRegister.RegisterAsync(clientCredToRegister, new Uri("http://localhost:8080/admin/realms/dev-realm/clients"), token);
            //}
            //catch (HttpRequestException ex)
            //{
            //    Console.WriteLine($"Error registering client: {ex.Message}");
            //}

            string oidcJson = """
                {
                  "client_name": "svc-client2",
                  "application_type": "confidential",
                  "grant_types": ["client_credentials"],
                  "token_endpoint_auth_method": "client_secret_basic"
                }
                """;

            JsonDocument oidcClientToRegister = JsonDocument.Parse(oidcJson);

            string initialToken = "eyJhbGciOiJIUzUxMiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICI4NTdkNjE4OS1jZmU4LTRmOGUtOTBmMi05OWJkNGExYzJhY2YifQ.eyJleHAiOjE3NzAwNjM2MDEsImlhdCI6MTc2OTk3NzIwMSwianRpIjoiMTNkMzEyYzItMDMzYi00NzQ4LTk1NGYtZDMyMTlkYmU3MjU3IiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDgwL3JlYWxtcy9kZXYtcmVhbG0iLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjgwODAvcmVhbG1zL2Rldi1yZWFsbSIsInR5cCI6IkluaXRpYWxBY2Nlc3NUb2tlbiJ9.V9v7EbKN2gnIRE63NGyGFEcikQfbJz3HZpZj4Ftf-lcWN1RMxCaEO1yil0qJftYD2lZLsUJSlnMlZbM-Weu70g";

            //try
            //{
            //    await clientRegister.RegisterAsync(oidcClientToRegister, new Uri(devDiscoveryDocument.RegistrationEndpoint), initialToken);
            //}
            //catch (HttpRequestException ex)
            //{
            //    Console.WriteLine($"Error registering client: {ex.Message}");
            //}

            ClientCredentialsFlowClient clientCredentialsFlowClient =
                new ClientCredentialsFlowClient(new Uri(devDiscoveryDocument.TokenEndpoint), "svc-client", "9rOkTdgyfDPgQ6lFN9LRewXLUkLYSxNt");

            string clientCredToken = await clientCredentialsFlowClient.GetTokenAsync();

            RealmRegister realmRegister = new RealmRegister();

            //await realmRegister.RegisterAsync(
            //    JsonDocument.Parse("""
            //    {
            //      "realm": "dev-realm",
            //      "enabled": true
            //    }
            //    """),
            //    new Uri("http://localhost:8080/admin/realms"),
            //    token);

            UserRegister userRegister = new UserRegister();

            string userToRegister = """
                {
                    "username": "john.doe",
                    "email": "john.doe@example.com",
                    "enabled": true,
                    "firstName": "John",
                    "lastName": "Doe",
                    "credentials": [{
                      "type": "password",
                      "value": "password"
                    }]
                }
                """;

            //await userRegister.RegisterAsync(
            //    JsonDocument.Parse(userToRegister), new Uri("http://localhost:8080/admin/realms/dev-realm/users"), token);

            //using (HttpClient client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //    string userInfoJson = await client.GetStringAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/users?username=john.doe"));

            //    JsonDocument userInfo = JsonDocument.Parse(userInfoJson);

            //    string userId = userInfo.RootElement[0].GetProperty("id").GetString();

            //    PasswordChanger passwordChanger = new PasswordChanger();
            //    await passwordChanger.ChangePasswordAsync(
            //        new Uri($"http://localhost:8080/admin/realms/dev-realm/users/{userId}/reset-password"),
            //        "newPassword123",
            //        token
            //    );

            //    ResourceOwnerPasswordCredentialsFlowClient UserROPC =
            //        new ResourceOwnerPasswordCredentialsFlowClient(new Uri(devDiscoveryDocument.TokenEndpoint), "ropc-client", "john.doe", "newPassword123");

            //    string userToken = await UserROPC.GetTokenAsync();
            //}

            //ClientInfoGetter clientInfoGetter = new ClientInfoGetter();
            //JsonDocument clientInfo = await clientInfoGetter.GetClientInfoAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/clients?clientId=ropc-client"), token);

            ;

            string userRegistrationClient = """
                {
                  "clientId": "User_Registration_Client",
                  "protocol": "openid-connect",
                  "enabled": true,
                  "publicClient": false,
                  "serviceAccountsEnabled": true,
                  "standardFlowEnabled": false,
                  "directAccessGrantsEnabled": false
                }
                """;

            //JsonDocument userRegistrationClientToRegister = JsonDocument.Parse(userRegistrationClient);

            ////await clientRegister
            ////    .RegisterAsync(userRegistrationClientToRegister, new Uri("http://localhost:8080/admin/realms/dev-realm/clients"), token);

            ClientInfoGetter clientInfoGetter = new ClientInfoGetter();
            JsonDocument userRegistrationClientInfo = await clientInfoGetter
                .GetClientInfoAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/clients?clientId=User_Registration_Client"), token);

            string userRegistrationClientId = userRegistrationClientInfo.RootElement[0].GetProperty("id").GetString();

            ServiceAccountRepository serviceAccountRepository = new ServiceAccountRepository(new Uri("http://localhost:8080/admin/realms/dev-realm"));

            JsonDocument serviceAccount = await serviceAccountRepository.GetAsync(userRegistrationClientId, token);

            string serviceAccountUserId = serviceAccount.RootElement.GetProperty("id").GetString();

            JsonDocument realmManagementClientInfo = await clientInfoGetter
                .GetClientInfoAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/clients?clientId=realm-management"), token);

            string realmManagementClientId = realmManagementClientInfo.RootElement[0].GetProperty("id").GetString();

            RoleRepository roleRepository = new RoleRepository(new Uri("http://localhost:8080/admin/realms/dev-realm/"), realmManagementClientId);

            JsonDocument manageUsersRole = await roleRepository.GetAsync("manage-users", token);

            JsonDocument asd = await roleRepository.GetUserRolesAsync(serviceAccountUserId, token);

            ClientCredentialsFlowClient userCreateionClientCred =
                new ClientCredentialsFlowClient(new Uri(devDiscoveryDocument.TokenEndpoint), "User_Registration_Client", "tM6IEzJ1whXnPLFZvoSHzBVDuXdEeSLE");

            string userRegToken = await userCreateionClientCred.GetTokenAsync();

            string userToWithClient = """
                {
                    "username": "john",
                    "email": "john@example.com",
                    "enabled": true,
                    "firstName": "John",
                    "lastName": "kaki",
                    "credentials": [{
                      "type": "password",
                      "value": "password"
                    }]
                }
                """;

            //await userRegister.RegisterAsync(
            //    JsonDocument.Parse(userToWithClient), new Uri("http://localhost:8080/admin/realms/dev-realm/users"), userRegToken);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string userInfoJson = await client.GetStringAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/users?username=john"));

                //string userInfoJson = await client.GetStringAsync(new Uri("http://localhost:8080/admin/realms/dev-realm/users?username=patkany"));

                JsonDocument userInfo = JsonDocument.Parse(userInfoJson);

                string userId = userInfo.RootElement[0].GetProperty("id").GetString();

                //PasswordChanger passwordChanger = new PasswordChanger();
                //await passwordChanger.ChangePasswordAsync(
                //    new Uri($"http://localhost:8080/admin/realms/dev-realm/users/{userId}/reset-password"),
                //    "newPassword1234",
                //    userRegToken
                //);

                ResourceOwnerPasswordCredentialsFlowClient UserROPC =
                    new ResourceOwnerPasswordCredentialsFlowClient(new Uri(devDiscoveryDocument.TokenEndpoint), "ropc-client", "john", "newPassword1234");

                string userToken = await UserROPC.GetTokenAsync();

                string myappWeb = """
                {
                  "clientId": "myapp-web",
                  "name": "My App Web Client",
                  "description": "Frontend client for My App",

                  "enabled": true,
                  "protocol": "openid-connect",

                  "publicClient": true,
                  "standardFlowEnabled": true,
                  "implicitFlowEnabled": false,
                  "directAccessGrantsEnabled": false,

                  "rootUrl": "http://localhost:3000",
                  "baseUrl": "http://localhost:3000",

                  "redirectUris": [
                    "http://localhost:3000/*"
                  ],

                  "webOrigins": [
                    "http://localhost:3000"
                  ],

                  "attributes": {
                    "post.logout.redirect.uris": "http://localhost:3000/*"
                  }
                }
                
                """;

                JsonDocument myappWebRegister = JsonDocument.Parse(myappWeb);

                await clientRegister.RegisterAsync(myappWebRegister, new Uri("http://localhost:8080/admin/realms/dev-realm/clients"), token);

                // http://localhost:8080/realms/dev-realm/protocol/openid-connect/auth?client_id=myapp-web&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2F&response_type=code&scope=openid&prompt=create
            }
        }
    }
}
