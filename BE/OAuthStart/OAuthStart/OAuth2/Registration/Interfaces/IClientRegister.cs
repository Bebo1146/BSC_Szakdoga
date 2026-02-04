using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthStart.OAuth2.Registration.Interfaces
{
    internal interface IClientRegister
    {
        Task RegisterAsync(string clientToRegister, Uri registerUrl, string accessToken,
            CancellationToken cancellationToken = default);
    }
}
