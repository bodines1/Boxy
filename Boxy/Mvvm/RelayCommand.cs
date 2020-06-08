using System;
using System.Windows.Input;

namespace CardMimic.Mvvm
{
    /// <summary>
    /// Command implementation which is bindable from views.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="methodToExecute">Main execute logic, and the Execute takes a parameter from the view.</param>
        /// <param name="canExecuteEvaluator">Logic which should evaluate whether the execute can be performed, and the CanExecute takes a parameter from the view.</param>
        public RelayCommand(Action<object> methodToExecute, Func<object, bool> canExecuteEvaluator = null)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="methodToExecute">Main execute logic.</param>
        /// <param name="canExecuteEvaluator">Logic which should evaluate whether the execute can be performed.</param>
        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator = null)
        {
            _methodToExecute = param => methodToExecute.Invoke();
            _canExecuteEvaluator = param => canExecuteEvaluator?.Invoke() ?? true;
        }

        private readonly Action<object> _methodToExecute;
        private readonly Func<object, bool> _canExecuteEvaluator;

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
            {
                return true;
            }

            bool result = _canExecuteEvaluator.Invoke(parameter);
            return result;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            _methodToExecute.Invoke(parameter);
        }
    }
}
