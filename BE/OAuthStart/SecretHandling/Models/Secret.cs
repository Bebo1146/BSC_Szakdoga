using System.Runtime.Serialization;

namespace SecretHandling.Models
{
    [Serializable]
    public sealed class Secret
    {
        [DataMember]
        private readonly byte[] _value;

        public byte[] Value => _value;

        public Secret(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length == 0)
            {
                throw new ArgumentException("The value length is 0.", nameof(value));
            }

            _value = value;
        }
    }
}
