using System;
using System.Windows.Input;

namespace EntryEditor.Models.Commands
{
    internal sealed class RelayCommand : IRelayCommand
    {
        private Action<object> _execute = null;
        private Predicate<object> _canExecute = null;

        public RelayCommand(Action<object> Execute, Predicate<object> CanExecute = null)
        {
            _execute = Execute;
            _canExecute = CanExecute;
        }

        public event EventHandler CanExecuteChanged;
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke(parameter = null);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            if (_canExecute != null)
            {
                CanExecuteChanged.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
