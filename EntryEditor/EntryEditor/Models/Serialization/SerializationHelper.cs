using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace EntryEditor.Models.Serialization
{
    internal class SerializationHelper<TEntry> : IDisposable
    {
        private ISerializer<IEnumerable<TEntry>> _serializer;
        private Stream _serializationStream;

        public SerializationHelper(ISerializer<IEnumerable<TEntry>> serializer)
        {
            _serializer = serializer;
        }

        public SerializationHelper()
        { }

        public void SetSerializer(ISerializer<IEnumerable<TEntry>> serializer)
        {
            _serializer = serializer;
        }

        public Stream SetSerializationStream(Stream stream) => _serializationStream = stream;

        public Stream GetSerializationStream() => _serializationStream;

        public Stream SerializationStream => _serializationStream;

        public void Serialize(IEnumerable<TEntry> entities)
        {
            if(entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            _serializer.SerializeToStream(_serializationStream, entities);
        }
            

        public IEnumerable<TEntry> Deserialize() => _serializer.DeserializeFromStream(_serializationStream);

        public void Dispose()
        {
            _serializationStream?.Dispose();
        }
    }
}
