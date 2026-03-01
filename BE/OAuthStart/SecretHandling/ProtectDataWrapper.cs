using System.Security.Cryptography;

namespace SecretHandling
{
    internal class ProtectDataWrapper
    {
        public byte[] Protect(byte[] data)
        {
            return ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return ProtectedData.Unprotect(protectedData, null, DataProtectionScope.LocalMachine);
        }
    }
}
