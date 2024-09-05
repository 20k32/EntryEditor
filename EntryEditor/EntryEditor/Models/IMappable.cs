using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryEditor.Models
{
    internal interface IMappable<T>
    {
        void Map(out T value);
    }
}
