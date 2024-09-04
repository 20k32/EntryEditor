using System.Windows.Input;

namespace EntryEditor.Models.Commands
{
    public interface IRelayCommand : ICommand
    {
        void NotifyCanExecuteChanged();
    }
}
