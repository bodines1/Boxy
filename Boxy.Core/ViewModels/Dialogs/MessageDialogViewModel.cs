using Boxy.Mvvm;

namespace Boxy.ViewModels.Dialogs
{
    /// <summary>
    /// View model for interacting with a message dialog window.
    /// </summary>
    public class MessageDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="MessageDialogViewModel"/>.
        /// </summary>
        /// <param name="message">Message to display in the dialog.</param>
        public MessageDialogViewModel(string message)
        {
            Message = message;
            Title = "Information";
        }

        /// <summary>
        /// Creates a new instance of <see cref="MessageDialogViewModel"/>.
        /// </summary>
        /// <param name="message">Message to display in the dialog.</param>
        /// <param name="title">Window title.</param>
        public MessageDialogViewModel(string message, string title)
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
