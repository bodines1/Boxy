﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.Mvvm;

namespace CellManager.ViewModels.MvvmUtilities
{
    /// <summary>
    /// An async command that implements <see cref="ICommand"/>, forwarding <see cref="ICommand.Execute(object)"/> to <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
    /// </summary>
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        /// <summary>
        /// The local implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        private readonly ICanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// Creates an instance with its own implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        protected AsyncCommandBase(Func<object, ICanExecuteChanged> canExecuteChangedFactory)
        {
            _canExecuteChanged = canExecuteChangedFactory(this);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public abstract Task ExecuteAsync(object parameter);

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected abstract bool CanExecute(object parameter);

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        protected void OnCanExecuteChanged()
        {
            _canExecuteChanged.OnCanExecuteChanged();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                _canExecuteChanged.CanExecuteChanged += value;
            }

            remove
            {
                _canExecuteChanged.CanExecuteChanged -= value;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }
    }
}
