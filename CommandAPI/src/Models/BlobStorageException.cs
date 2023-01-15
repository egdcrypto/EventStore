using System.Runtime.Serialization;

namespace CommandAPI.Models
{
    [Serializable]
    internal class BlobStorageException : Exception
    {
        public BlobStorageException()
        {
        }

        public BlobStorageException(string message) : base(message)
        {
        }

        public BlobStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BlobStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Content { get; set; }
    }
}