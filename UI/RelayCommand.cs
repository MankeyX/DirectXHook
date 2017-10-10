using System;
using System.Windows.Input;

namespace UI
{
    public class RelayCommand : ICommand
    {
        private readonly Action _targetMethod;
        private readonly Func<bool> _targetCanExecuteMethod;

        public RelayCommand(Action method)
        {
            _targetMethod = method;
        }

        public RelayCommand(Action method, Func<bool> canExecuteMethod)
        {
            _targetMethod = method;
            _targetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (_targetCanExecuteMethod != null)
                return _targetCanExecuteMethod();

            return _targetMethod != null;
        }

        public void Execute(object parameter)
        {
            _targetMethod?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }
}
