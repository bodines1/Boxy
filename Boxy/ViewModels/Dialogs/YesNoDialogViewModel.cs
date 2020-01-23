using Boxy.Mvvm;

namespace Boxy.ViewModels.Dialogs
{
    /// <summary>
    /// View model for interacting with a message dialog window.
    /// </summary>
    public class YesNoDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="YesNoDialogViewModel"/>.
        /// </summary>
        /// <param name="message">Message to display in the dialog.</param>
        public YesNoDialogViewModel(string message)
        {
            Message = message;
            Title = "Continue?";
        }

        /// <summary>
        /// Creates a new instance of <see cref="YesNoDialogViewModel"/>.
        /// </summary>
        /// <param name="message">Message to display in the dialog.</param>
        /// <param name="title">Window title.</param>
        public YesNoDialogViewModel(string message, string title)
        {
            Message = message;
            Title = title;
        }

        /// <summary>
        /// Message to display in the dialog.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Window title.
        /// </summary>
        public string Title { get; }
    }
}
