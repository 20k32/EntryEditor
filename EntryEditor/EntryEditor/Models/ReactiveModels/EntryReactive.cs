using EntryEditor.Models.ReactiveModels;
using EntryEditor.Models.Serialization;
using EntryEditor.Models.Serialization.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace EntryEditor.Models
{
    internal sealed class EntryReactive : ReactiveModelBase,
        INotifyPropertyChanged,
        IMappable<EntryDTO>,
        IEditableObject
    {
        private static readonly SerializationHelper<EntryDTO> serializationHelper = new();

        private string firstNameBacking;
        private string lastNameBacking;

        public EntryReactive()
        {
            Id = Guid.NewGuid();
            canEdit = true;
        }

        public void InitCredentialsIfAllowed(string firstName, string lastName)
        {
            if (CanEdit)
            {
                firstNameBacking = this.firstName = firstName;
                lastNameBacking = this.lastName = lastName;
                canEdit = false;
            }
        }

        public Guid Id { get; private set; }

        #region First Name field

        private string firstName = string.Empty;
        public string FirstName
        {
            get => firstName;
            set
            {
                if (CanEdit)
                {
                    firstName = value;
                    OnPropertyChanged();
                    ValidateFirstName();
                }
            }
        }

        #endregion First Name field

        #region Last Name field

        private string lastName = string.Empty;
        public string LastName
        {
            get => lastName;
            set
            {
                if (CanEdit)
                {
                    lastName = value;
                    OnPropertyChanged();
                    ValidateLastName();
                }
            }
        }

        #endregion Last Name Field

        #region Can Edit Fields

        private bool canEdit = false;

        public bool CanEdit
        {
            get => canEdit;
            set
            {
                if (canEdit != value)
                {
                    canEdit = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is EntryReactive entry)
            {
                result = Equals(entry);
            }
            else if (obj is Guid id)
            {
                result = Equals(id);
            }

            return result;
        }

        public bool Equals(EntryReactive entry) => Id.Equals(entry.Id);
        public bool Equals(Guid id) => Id.Equals(id);

        public override int GetHashCode() => Id.GetHashCode();

        public void Map(out EntryDTO value) =>
            value = CanEdit
            ? new(firstNameBacking, lastNameBacking)
            : new(FirstName, LastName);

        public static void Serialize(Stream stream, IEnumerable<EntryReactive> entries, int count = 0)
        {
            if (entries is null)
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
            if (callback is null)
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
                        if (entry.FirstName is null || entry.LastName is null)
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

        public void BeginEdit()
        {
            if (IsValid)
            {
                CanEdit = true;
                firstNameBacking = FirstName;
                lastNameBacking = LastName;
            }
        }

        public void CancelEdit()
        {
            if (CanEdit)
            {
                CanEdit = false;
                firstName = firstNameBacking;
                lastName = lastNameBacking;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(LastName));

                ClearErrors();
                OnErrorsChanged(this, new(nameof(FirstName)));
                OnErrorsChanged(this, new(nameof(LastName)));
            }
        }

        public void EndEdit()
        {
            if (CanEdit && IsValid)
            {
                CanEdit = false;
                firstNameBacking = FirstName;
                lastNameBacking = LastName;
            }
        }

        #region Validation

        public bool IsValid => !HasErrors;

        private void ValidateFirstName()
        {
            if (string.IsNullOrEmpty(FirstName))
            {
                AddError(nameof(FirstName), "First name is null");
            }
            else
            {
                DeleteError(nameof(FirstName), "First name is null");
            }

            OnErrorsChanged(this, new(nameof(FirstName)));
            OnPropertyChanged(nameof(IsValid));
        }

        private void ValidateLastName()
        {
            if (string.IsNullOrEmpty(LastName))
            {
                AddError(nameof(LastName), "Last name is null");
            }
            else
            {
                DeleteError(nameof(LastName), "Last name is null");
            }

            OnErrorsChanged(this, new(nameof(LastName)));
            OnPropertyChanged(nameof(IsValid));
        }

        #endregion
    }
}