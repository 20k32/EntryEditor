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

        private static readonly MessageDialog dialogWindow = new(string.Empty);

        public static IAsyncOperation<IUICommand> ShowMessageAsync(string message, params string[] buttonNames)
        {
            if(buttonNames is null)
            {
                throw new ArgumentNullException(nameof(buttonNames));
            }

            dialogWindow.Commands.Clear();
            dialogWindow.Content = message;
            
            foreach(var name in buttonNames)
            {
                dialogWindow.Commands.Add(new UICommand(name));
            }


            return dialogWindow.ShowAsync();
        }

    }
}
