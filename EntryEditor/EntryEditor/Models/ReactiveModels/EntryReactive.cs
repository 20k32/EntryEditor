using EntryEditor.Models.Serialization;
using EntryEditor.Models.Serialization.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Email.DataProvider;


namespace EntryEditor.Models
{
    internal sealed class EntryReactive : INotifyPropertyChanged, IMappable<EntryDTO>
    {
        private static readonly SerializationHelper<EntryDTO> _serializationHelper = new(new XmlSerializer());

        public EntryReactive()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        private string _fristName;
        public string FirstName
        {
            get => _fristName;
            set
            {
                _fristName = value;
                OnPropertyChanged();
            }
        }

        private string _lastName;
        public string LastName 
        { 
            get => _lastName; 
            set
            {
                _lastName = value;
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

        public void Map(out EntryDTO value) => value = new(FirstName, LastName);

        private void RestoreState(in EntryDTO state)
        {
            FirstName = state.FirstName;
            LastName = state.LastName;
        }

        public static void Serialize(Stream stream, IEnumerable<EntryReactive> entries, int count = 0)
        {
            if(entries is null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            List<EntryDTO> entriesToSerialize = new(count);
            foreach (EntryReactive item in entries)
            {
                item.Map(out var entry);
                entriesToSerialize.Add(entry);
            }

            using (_serializationHelper.SetSerializationStream(stream))
            {
                _serializationHelper.Serialize(entriesToSerialize);
            }
        }

        public static void Deserialize(Stream stream, Action<IEnumerable<EntryReactive>> callback)
        {
            if(callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            using (_serializationHelper.SetSerializationStream(stream))
            {
                var entries = _serializationHelper
                    .Deserialize()
                    .Select(entry =>
                    {
                        var entryReactive = new EntryReactive();
                        entryReactive.RestoreState(in entry);
                        return entryReactive;
                    });

                callback(entries);
            }
        }
    }
}