using System;

namespace Boxy.Reporting
{
    /// <summary>
    /// Event args to report a status message to a listener.
    /// </summary>
    public class CardMimicStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the EventArgs.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="isError">Whether this status update was from an error.</param>
        public CardMimicStatusEventArgs(string message, bool isError)
        {
            Message = message;
            IsError = isError;
        }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Bool indicating whether the event which caused the status update was an error.
        /// </summary>
        public bool IsError { get; }
    }
}
