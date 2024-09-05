using System.Runtime.Serialization;
using Windows.Storage;

namespace EntryEditor.Models.Serialization.DTOs
{
    [DataContract]
    internal readonly struct EntryDTO
    {
        [DataMember]
        public readonly string FirstName;

        [DataMember]
        public readonly string LastName;

        public EntryDTO(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
