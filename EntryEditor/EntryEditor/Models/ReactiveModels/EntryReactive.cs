using EntryEditor.Models.Serialization;
using EntryEditor.Models.Serialization.DTOs;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Windows.ApplicationModel.Email.DataProvider;
using Windows.Storage.Streams;


namespace EntryEditor.Models
{
    internal sealed class EntryReactive : INotifyPropertyChanged, IMappable<EntryDTO>
    {
        private string firstNameBacking;
        private string lastNameBacking;

        private static readonly SerializationHelper<EntryDTO> _serializationHelper = new();

        public EntryReactive()
        {
            Id = Guid.NewGuid();
            allowEditing = true;
        }

        public void InitCredentials(string firstName, string lastName)
        {
            if (AllowEditing)
            {
                firstNameBacking = _fristName = firstName;
                lastNameBacking = _lastName = lastName;
                allowEditing = false;
            }
        }

        public Guid Id { get; private set; }

        private string _fristName = string.Empty;
        public string FirstName
        {
            get => _fristName;
            set
            {
                if (AllowEditing && !_fristName.Equals(value))
                {
                    _fristName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _lastName = string.Empty;
        public string LastName 
        { 
            get => _lastName; 
            set
            {
                if(AllowEditing && _lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool allowEditing = false;

        public bool AllowEditing
        {
            get => allowEditing;
            set
            {
                if(allowEditing != value)
                {
                    allowEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        public void CommitChanges()
        {
            firstNameBacking = FirstName;
            lastNameBacking = LastName;
        }

        public void RollBackChanges()
        {
            FirstName = firstNameBacking;
            LastName = lastNameBacking;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            if(obj is EntryReactive entry)
            {
                result = Equals(entry);
            }
            else if(obj is Guid id)
            {
                result = Equals(id);
            }

            return result;
        }

        public bool Equals(EntryReactive entry) => Id.Equals(entry.Id);
        public bool Equals(Guid id) => Id.Equals(id);

        public override int GetHashCode() => Id.GetHashCode();

        public void Map(out EntryDTO value) => value = new(FirstName, LastName);

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
                        if(entry.FirstName is null || entry.LastName is null)
                        {
                            throw new SerializationException("Could not deserialize entity.");
                        }

                        entryReactive.InitCredentials(entry.FirstName, entry.LastName);
                        return entryReactive;
                    });

                callback(entries);
            }
        }

        public static void SetSerializer(ISerializer<IEnumerable<EntryDTO>> serializer)
            => _serializationHelper.SetSerializer(serializer);
    }
}