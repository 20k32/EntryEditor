using System.Runtime.Serialization;
using Windows.Storage;

namespace EntryEditor.Models.Serialization.DTOs
{
    [DataContract]
    internal readonly struct EntryDTO
    {
        [DataMember(Name = "fn")]
        public readonly string FirstName;

        [DataMember(Name = "ln")]
        public readonly string LastName;

        public EntryDTO(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
