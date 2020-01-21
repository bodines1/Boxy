using CellManager.Resources.DialogService;
using CellManager.ViewModels.MvvmUtilities;
using System;

namespace CellManager.Resources.MvvmUtilities
{
    /// <summary>
    /// Has implementations to tie in with an <see cref="IDialog"/>.
    /// </summary>
    public class DialogViewModelBase : ViewModelBase, IDialogRequestClose
    {
        /// <summary>
        /// Event fires when a close is requested of the dialog window.
        /// </summary>
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        /// <summary>
        /// Ends a dialog session with the resulting arguments.
        /// </summary>
        /// <param name="args">contains the dialog result.</param>
        protected void RequestClose(DialogCloseRequestedEventArgs args)
        {
            CloseRequested?.Invoke(this, args);
        }

        #region Commands

        #region OkDialog

        private RelayCommand _okDialog;

        /// <summary>
        /// Executes a close event with a true result.
        /// </summary>
        public RelayCommand OkDialog
        {
            get
            {
                return _okDialog ?? (_okDialog = new RelayCommand(OkDialog_Execute, OkDialog_CanExecute));
            }
        }

        /// <summary>
        /// Execution logic for the OkDialog relay command.
        /// </summary>
        protected virtual void OkDialog_Execute()
        {
            RequestClose(new DialogCloseRequestedEventArgs(true));
        }

        /// <summary>
        /// Logic to determine if <see cref="OkDialog_Execute"/> can execute.
        /// </summary>
        /// <returns>Bool indicating whether execution is allowed.</returns>
        protected virtual bool OkDialog_CanExecute()
        {
            return true;
        }

        #endregion OkDialog

        #region CancelDialog

        private RelayCommand _cancelDialog;

        /// <summary>
        /// Executes a close event with a false result.
        /// </summary>
        public RelayCommand CancelDialog
        {
            get
            {
                return _cancelDialog ??
                       (_cancelDialog = new RelayCommand(CancelDialog_Execute, CancelDialog_CanExecute));
            }
        }

        /// <summary>
        /// Execution logic for the CancelDialog relay command.
        /// </summary>
        protected virtual void CancelDialog_Execute()
        {
            RequestClose(new DialogCloseRequestedEventArgs(false));
        }

        /// <summary>
        /// Logic to determine if <see cref="CancelDialog_Execute"/> can execute.
        /// </summary>
        /// <returns>Bool indicating whether execution is allowed.</returns>
        protected virtual bool CancelDialog_CanExecute()
        {
            return true;
        }

        #endregion CancelDialog

        #endregion Commands
    }
}
