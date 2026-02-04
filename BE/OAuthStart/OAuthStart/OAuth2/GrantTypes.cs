using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthStart.OAuth2HttpCommunication
{
    internal class GrantTypes
    {
        public const string AuthorizationCode = "authorization_code";
        public const string ClientCredentials = "client_credentials";
        public const string ResourceOwnerPasswordCredentials = "password";
        public const string RefreshToken = "refresh_token";
        public const string Implicit = "implicit";
        public const string DeviceCode = "urn:ietf:params:oauth:grant-type:device_code";
    }
}
