using System;
using System.ComponentModel;

namespace CellManager.Resources.Reporting
{
    public interface IReporter : IProgress<string>, INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates whether this is currently in a process requiring reporting.
        /// </summary>
        bool IsSystemBusy { get; }

        /// <summary>
        /// Sets the system busy state to true.
        /// </summary>
        void StartBusy();

        /// <summary>
        /// Sets the system busy state to false.
        /// </summary>
        void StopBusy();

        /// <summary>
        /// Reports the error encurred to subscribers.
        /// </summary>
        /// <param name="sender">Object that reported the error.</param>
        /// <param name="exc">Exception thrown.</param>
        /// <param name="suppressDisplay">Requests that a message box is not displayed but just a record.</param>
        void ReportError(object sender, Exception exc, bool suppressDisplay = false);

        /// <summary>
        /// Reports the error encurred to subscribers.
        /// </summary>
        /// <param name="sender">Object that reported the error.</param>
        /// <param name="message">Description of the error.</param>
        /// <param name="suppressDisplay">Requests that a message box is not displayed but just a record.</param>
        void ReportError(object sender, string message, bool suppressDisplay = false);

        /// <summary>
        /// Reports the error encurred to subscribers.
        /// </summary>
        /// <param name="sender">Object that reported the error.</param>
        /// <param name="exc">Exception thrown.</param>
        /// <param name="message">Description of the error.</param>
        /// <param name="suppressDisplay">Requests that a message box is not displayed but just a record.</param>
        void ReportError(object sender, Exception exc, string message, bool suppressDisplay = false);

        /// <summary>
        /// Event raised when an error has been recieved.
        /// </summary>
        event EventHandler<CellManagerErrorEventArgs> ErrorReported;

        /// <summary>
        /// Event handler for progress changed.
        /// </summary>
        event EventHandler<string> StatusReported;
    }
}
