using System;
using System.ComponentModel;

namespace Boxy.Reporting
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
        /// Reports the status to subscribers.
        /// </summary>
        /// <param name="sender">Object reporting the status update.</param>
        /// <param name="message">The message.</param>
        /// <param name="isError">Bool indicating whether the event which caused the status update was an error.</param>
        void Report(object sender, string message, bool isError = false);

        /// <summary>
        /// Event raised when an error has been recieved.
        /// </summary>
        event EventHandler<BoxyStatusEventArgs> StatusReported;
    }
}
