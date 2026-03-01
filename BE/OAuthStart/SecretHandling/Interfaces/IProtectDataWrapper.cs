namespace SecretHandling.Interfaces
{
    internal interface IProtectDataWrapper
    {
        byte[] Protect(byte[] data);
        byte[] Unprotect(byte[] protectedData);
    }
}
