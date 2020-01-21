using System;

namespace CellManager.Resources.DialogService
{
    /// <summary>
    /// Event args which contains a result indicating the way a dialog closes.
    /// </summary>
    public class DialogCloseRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Instantiates a new <see cref="DialogCloseRequestedEventArgs"/>.
        /// </summary>
        /// <param name="dialogResult">result from the dialog.</param>
        public DialogCloseRequestedEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }

        /// <summary>
        /// The result from the dialog.
        /// </summary>
        public bool? DialogResult { get; }
    }
}
