using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace EntryEditor.Models.Serialization
{
    internal sealed class SerializationHelper<TEntry> : IDisposable
    {
        private ISerializer<IEnumerable<TEntry>> serializer;
        private Stream serializationStream;

        public SerializationHelper(ISerializer<IEnumerable<TEntry>> serializer)
            => this.serializer = serializer;

        public SerializationHelper()
        { }

        public void SetSerializer(ISerializer<IEnumerable<TEntry>> serializer) 
            => this.serializer = serializer;

        public Stream SetSerializationStream(Stream stream) => serializationStream = stream;

        public Stream GetSerializationStream() => serializationStream;

        public Stream SerializationStream => serializationStream;

        public void Serialize(IEnumerable<TEntry> entities) 
            => serializer.SerializeToStream(serializationStream, entities);

        public IEnumerable<TEntry> Deserialize() => serializer.DeserializeFromStream(serializationStream);

        public void Dispose()
        {
            serializationStream?.Dispose();
        }
    }
}
