using EntryEditor.Models.Serialization.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace EntryEditor.Models.Serialization
{
    internal class JsonSerializer : ISerializer<IEnumerable<EntryDTO>>
    {
        private readonly DataContractJsonSerializer serializer;

        public JsonSerializer()
        {
            serializer = new(typeof(IEnumerable<EntryDTO>));
        }

        public IEnumerable<EntryDTO> DeserializeFromStream(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return (IEnumerable<EntryDTO>)serializer.ReadObject(stream);
        }

        public void SerializeToStream(Stream stream, IEnumerable<EntryDTO> entity)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            serializer.WriteObject(stream, entity);
        }
    }
}
