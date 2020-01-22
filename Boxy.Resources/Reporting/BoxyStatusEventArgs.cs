using System;

namespace Boxy.Resources.Reporting
{
    /// <summary>
    /// Event args to report a status message to a listener.
    /// </summary>
    public class BoxyStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the EventArgs.
        /// </summary>
        /// <param name="sender">Class which originated the status update call.</param>
        /// <param name="message">Message to display.</param>
        /// <param name="isError">Whether this status update was from an error.</param>
        public BoxyStatusEventArgs(object sender, string message, bool isError)
        {
            Sender = sender;
            Message = message;
            IsError = isError;
        }

        /// <summary>
        /// Class which originated the status update call.
        /// </summary>
        public object Sender { get; }

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
