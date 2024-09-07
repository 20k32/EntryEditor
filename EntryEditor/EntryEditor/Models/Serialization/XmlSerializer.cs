using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using EntryEditor.Models.Serialization.DTOs;

namespace EntryEditor.Models.Serialization
{
    internal sealed class XmlSerializer : ISerializer<IEnumerable<EntryDTO>>
    {
        private readonly DataContractSerializer serializer;

        public XmlSerializer()
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
            if(stream is null)
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
