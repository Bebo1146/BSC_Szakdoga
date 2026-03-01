using SecretHandling.ExtensionMethods;
using SecretHandling.Interfaces;
using SecretHandling.Models;

namespace SecretHandling
{
    public class SecretRepository
    {
        internal static readonly string SECRET_REPOSITORY_FILE_PATH = Path.Combine(Environment.ExpandEnvironmentVariables(@"%APPDATA%\OAuth2\ClientRepository"), "data.dat");
        private IFileSystemHandler _fileSystemHandler;
        private IProtectDataWrapper _protectDataWrapper;

        internal SecretRepository(IFileSystemHandler fileSystemHandler, IProtectDataWrapper protectDataWrapper)
        {
            _fileSystemHandler = fileSystemHandler;
            _protectDataWrapper = protectDataWrapper;
        }

        /// <summary>
        /// The Write method is designed to store a Secret object associated with a given clientId into a repository.
        /// </summary>
        /// <param name="clientId">Represents the unique identifier for a client</param>
        /// <param name="secret">Represents a sensitive object containing the secret information for the client.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Write(string clientId, Secret secret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("The clientId is null or contains whitespaces only.", nameof(clientId));
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            Dictionary<string, Secret> fileContentDictionary = new Dictionary<string, Secret>();

            if (_fileSystemHandler.FileExists(SECRET_REPOSITORY_FILE_PATH))
            {
                fileContentDictionary = ReadFile().ToDictionary();
            }

            fileContentDictionary[clientId] = secret;
            WriteFile(fileContentDictionary.ToByteArray());
        }

        /// <summary>
        /// The Read method is designed to retrieve a Secret object associated with a given clientId from a repository.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client whose Secret is to be retrieved.</param>
        /// <returns>The Secret associated with the provided clientId if it exists.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Secret Read(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("The clientId is null or contains whitespaces only.", nameof(clientId));
            }

            Dictionary<string, Secret> fileContentDictionary = ReadFile().ToDictionary();

            if (!fileContentDictionary.TryGetValue(clientId, out Secret secret))
            {
                throw new ArgumentException("The given clientId does not exist", nameof(clientId));
            }

            return secret;
        }

        private byte[] ReadFile()
        {
            byte[] protectedData = _fileSystemHandler.ReadFile(SECRET_REPOSITORY_FILE_PATH);
            if (protectedData.Length > 0)
            {
                return _protectDataWrapper.Unprotect(protectedData);
            }

            return new byte[] { };
        }

        private void WriteFile(byte[] data)
        {
            byte[] encryptedData = _protectDataWrapper.Protect(data);

            data.Clear();

            _fileSystemHandler.WriteFile(SECRET_REPOSITORY_FILE_PATH, encryptedData);
        }
    }
}
