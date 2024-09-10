using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth.Advertisement;

namespace EntryEditor.Models
{
    internal sealed class ErrorCollection
    {
        private readonly Dictionary<string, HashSet<string>> errors;

        public ErrorCollection()
        {
            errors = new();
        }

        public void AddError(string key, string error)
        {
            if(errors.TryGetValue(key, out var result))
            {
                result.Add(error);
            }
            else
            {
                errors.Add(key, new() { error });
            }
        }

        public IEnumerable<string> GetErrors(string key)
        {
            IEnumerable<string> result = null;

            if(errors.TryGetValue(key, out var value))
            {
                result = value;
            }

            return result;
        }

        public void DeleteError(string key, string message)
        {
            if (errors.TryGetValue(key, out var value))
            {
                value.Remove(message);
            }
        }

        public void ClearErrors(string key) 
        {
            if (errors.TryGetValue(key, out var result))
            {
                result.Clear();
            }
        }

        public bool HasErrors() 
        {
            bool result = false;

            foreach(var error in errors)
            {
                if(error.Value.Count > 0)
                {
                    result = true; 
                }
            }

            return result;
        }

        public void Clear() => errors.Clear();
    }
}
