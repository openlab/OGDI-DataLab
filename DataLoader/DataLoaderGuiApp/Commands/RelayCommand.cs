using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Ogdi.Data.DataLoaderGuiApp.Commands
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion

        #region Fields

        private readonly Predicate<object> _canExecute;

        private readonly Action<object> _execute;

        #endregion
    }
}