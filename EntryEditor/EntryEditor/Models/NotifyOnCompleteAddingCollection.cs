using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EntryEditor.Models
{
    internal sealed class NotifyOnCompleteAddingCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> range)
        {
            foreach(var item in range)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
