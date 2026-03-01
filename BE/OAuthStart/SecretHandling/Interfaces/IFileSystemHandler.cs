namespace SecretHandling.Interfaces
{
    internal interface IFileSystemHandler
    {
        void WriteFile(string filePath, byte[] data);

        byte[] ReadFile(string filePath);

        void CreateDirectory(string directoryPath);

        bool FileExists(string filePath);
    }
}
