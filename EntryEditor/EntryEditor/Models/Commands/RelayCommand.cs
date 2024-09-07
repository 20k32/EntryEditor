using System;
using System.Windows.Input;

namespace EntryEditor.Models.Commands
{
    internal sealed class RelayCommand : IRelayCommand
    {
        private Action<object> execute = null;
        private Predicate<object> canExecute = null;

        public RelayCommand(Action<object> Execute, Predicate<object> CanExecute = null)
        {
            execute = Execute;
            canExecute = CanExecute;
        }

        public event EventHandler CanExecuteChanged;
        
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            execute?.Invoke(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            if (canExecute != null)
            {
                CanExecuteChanged.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
