using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace EntryEditor.Models.ReactiveModels
{
    internal class ReactiveModelBase : INotifyDataErrorInfo
    {
        private ErrorCollection errors;

        public ReactiveModelBase()
        {
            errors = new();
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => errors.HasErrors();

        public IEnumerable GetErrors(string propertyName) => errors.GetErrors(propertyName);

        protected void AddError(string key, string errorMessage) 
            => errors.AddError(key, errorMessage);

        protected void ClearErrorsByKey(string key) => errors.ClearErrors(key);

        protected void ClearErrors() => errors.Clear();

        protected void DeleteError(string key, string errorMessage) 
            => errors.DeleteError(key, errorMessage);

        protected void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) 
            => ErrorsChanged?.Invoke(sender, e);
    }
}
