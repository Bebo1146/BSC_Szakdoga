using SecretHandling.Interfaces;

namespace SecretHandling
{
    internal class FileSystemHandler : IFileSystemHandler
    {
        private static object _staticLock = new object();

        public void WriteFile(string filePath, byte[] data)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                CreateDirectory(dir);
            }
            lock (_staticLock)
            {
                using (FileStream fStream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    if (fStream.CanWrite)
                    {
                        fStream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        string message = "Data could not be written the stream.";

                        throw new IOException(message);
                    }
                }

                SetUpFileAccess(filePath);
            }
        }

        public byte[] ReadFile(string filePath)
        {
            lock (_staticLock)
            {
                FileInfo fileInfo = new FileInfo(filePath);

                try
                {
                    int length = (int)fileInfo.Length;

                    using (FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] protectedData = new byte[length];

                        if (fStream.CanRead)
                        {
                            fStream.Read(protectedData, 0, length);
                        }
                        else
                        {
                            string message = "Data could not be read.";

                            throw new IOException(message);
                        }

                        return protectedData;
                    }
                }
                catch (FileNotFoundException)
                {
                    //Ignore if file does not exist.
                    //This is required because File.Exists and FileInfo.Exists do not throw 
                    //UnauthorizedAccessException it only returns false as if the file does not exist.
                    string message = "File could not be found or required privileges for accessing are missing.";

                    return new byte[] { };
                }
            }
        }

        public void CreateDirectory(string directoryPath)
        {
            lock (_staticLock)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                if (!directoryInfo.Exists)
                {
                    directoryInfo = Directory.CreateDirectory(directoryPath);
                }

                // Set hidden attribute to folder
                if (!directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    directoryInfo.Attributes |= FileAttributes.Hidden;
                }
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// We need this to ensure file protection and access because inheritance is buggy and sometimes
        /// access rules disappear. 
        /// </summary>
        /// <param name="filePath"></param>
        private void SetUpFileAccess(string filePath)
        {
            File.SetAttributes(filePath, FileAttributes.Hidden);
        }
    }
}
