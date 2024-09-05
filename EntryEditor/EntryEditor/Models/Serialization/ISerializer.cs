using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryEditor.Models.Serialization
{
    internal interface ISerializer<TEntity>
    {
        void SerializeToStream(Stream stream, TEntity entity);
        TEntity DeserializeFromStream(Stream stream);
    }
}
