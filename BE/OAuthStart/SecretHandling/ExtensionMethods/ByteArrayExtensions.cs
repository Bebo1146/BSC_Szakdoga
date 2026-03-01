using System.Runtime.Serialization.Json;
using System.Text;
using SecretHandling.Models;

namespace SecretHandling.ExtensionMethods
{
    internal static class ByteArrayExtensions
    {
        private static DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(List<KeyValuePair<string, Secret>>));

        public static Dictionary<string, Secret> ToDictionary(this byte[] byteArray)
        {
            List<KeyValuePair<string, Secret>> retValue = new List<KeyValuePair<string, Secret>>();
            if (byteArray.Length > 0)
            {
                Stream stream = new MemoryStream(byteArray);
                stream.Position = 0;
                retValue = _serializer.ReadObject(stream) as List<KeyValuePair<string, Secret>>;
            }

            Dictionary<string, Secret> a = new Dictionary<string, Secret>();
            foreach (var item in retValue)
            {
                a.Add(item.Key, item.Value);
            }

            return a;
        }

        public static byte[] ToByteArray(this Dictionary<string, Secret> dictionary)
        {
            List<KeyValuePair<string, Secret>> retValue = new List<KeyValuePair<string, Secret>>();

            foreach (var item in dictionary)
            {
                retValue.Add(new KeyValuePair<string, Secret>(item.Key, item.Value));
            }

            MemoryStream stream = new MemoryStream();
            _serializer.WriteObject(stream, retValue);
            stream.Position = 0;

            using (StreamReader sr = new StreamReader(stream))
            {
                return Encoding.UTF8.GetBytes(sr.ReadToEnd());
            }
        }

        public static void Clear(this byte[] data)
        {
            Array.Clear(data, 0, data.Length);
        }
    }
}
