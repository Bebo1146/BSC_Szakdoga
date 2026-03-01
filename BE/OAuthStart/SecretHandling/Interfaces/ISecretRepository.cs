using SecretHandling.Models;

namespace SecretHandling.Interfaces
{
    internal interface ISecretRepository
    {
        Secret Read(string clientId);

        void Write(string clientId, Secret secret);
    }
}
