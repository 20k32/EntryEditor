using EntryEditor.Models.Serialization;
using EntryEditor.Models.Serialization.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace EntryEditor.Models
{
    internal sealed class EntryReactive : INotifyPropertyChanged, IMappable<EntryDTO>
    {
        private static readonly SerializationHelper<EntryDTO> serializationHelper = new();
        
        private string firstNameBacking;
        private string lastNameBacking;

        public EntryReactive()
        {
            Id = Guid.NewGuid();
            allowEditing = true;
        }

        public void InitCredentialsIfAllowed(string firstName, string lastName)
        {
            if (AllowEditing)
            {
                firstNameBacking = this.firstName = firstName;
                lastNameBacking = this.lastName = lastName;
                allowEditing = false;
            }
        }

        public Guid Id { get; private set; }

        private string firstName = string.Empty;
        public string FirstName
        {
            get => firstName;
            set
            {
                if (AllowEditing && firstName != value)
                {
                    firstName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string lastName = string.Empty;
        public string LastName 
        { 
            get => lastName; 
            set
            {
                if(AllowEditing && lastName != value)
                {
                    lastName = value;
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

        public void RollbackChanges()
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

        public void Map(out EntryDTO value) => 
            value = AllowEditing 
            ? new(firstNameBacking, lastNameBacking)
            : new (FirstName, LastName);

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

            using (serializationHelper.SetSerializationStream(stream))
            {
                serializationHelper.Serialize(entriesToSerialize);
            }
        }

        public static void Deserialize(Stream stream, Action<IEnumerable<EntryReactive>> callback)
        {
            if(callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            using (serializationHelper.SetSerializationStream(stream))
            {
                var entries = serializationHelper
                    .Deserialize()
                    .Select(entry =>
                    {
                        var entryReactive = new EntryReactive();
                        if(entry.FirstName is null || entry.LastName is null)
                        {
                            throw new SerializationException("Could not deserialize entity.");
                        }

                        entryReactive.InitCredentialsIfAllowed(entry.FirstName, entry.LastName);
                        return entryReactive;
                    });

                callback(entries);
            }
        }

        public static void SetSerializer(ISerializer<IEnumerable<EntryDTO>> serializer)
            => serializationHelper.SetSerializer(serializer);
    }
}