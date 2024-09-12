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

        [DataMember(Name = "md")]
        public readonly string ModifiedDate;

        public EntryDTO(string firstName, string lastName, string modifiedDate)
        {
            FirstName = firstName;
            LastName = lastName;
            ModifiedDate = modifiedDate;
        }
    }
}
