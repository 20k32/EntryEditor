using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EntryEditor.Models
{
    internal sealed class EntryReactive : INotifyPropertyChanged, IEntry
    {
        private Entry _entry;

        public EntryReactive()
        {
            _entry = new Entry();
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public string FirstName
        {
            get => _entry.FirstName;
            set
            {
                _entry.FirstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName 
        { 
            get => _entry.LastName; 
            set
            {
                _entry.LastName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if(obj is EntryReactive entry)
            {
                return EntryEquals(entry);
            }
            if(obj is Guid id)
            {
                return GuidEquals(id);
            }

            return false;
        }

        public bool EntryEquals(EntryReactive entry) => Id.Equals(entry.Id);
        public bool GuidEquals(Guid id) => Id.Equals(id);

        public override int GetHashCode() => Id.GetHashCode();
    }
}