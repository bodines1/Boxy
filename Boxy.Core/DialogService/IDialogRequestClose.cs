using System;

namespace Boxy.DialogService
{
    /// <summary>
    /// Interface for interacting with the close feature of a dialog.
    /// </summary>
    public interface IDialogRequestClose
    {
        /// <summary>
        /// Event signaling the close of a dialog.
        /// </summary>
        event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
