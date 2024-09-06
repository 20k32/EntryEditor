using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;

namespace EntryEditor.Models
{
    internal static class MessageDialogExtensions
    {
        public const string OkDialogButtonName = "Ok";
        public const string NoDialogButtonName = "No";
        public const string YesDialogButtonName = "Yes";

        private static readonly MessageDialog _dialogWindow = new(string.Empty);

        public static IAsyncOperation<IUICommand> ShowMessageAsync(string message, params string[] buttonNames)
        {
            if(buttonNames is null)
            {
                throw new ArgumentNullException(nameof(buttonNames));
            }

            _dialogWindow.Commands.Clear();
            _dialogWindow.Content = message;
            
            foreach(var name in buttonNames)
            {
                _dialogWindow.Commands.Add(new UICommand(name));
            }


            return _dialogWindow.ShowAsync();
        }

    }
}
