using System;
using System.ComponentModel;

namespace CardMimic.Reporting
{
    public interface IReporter : IProgress<string>, INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates whether this is currently in a process requiring reporting.
        /// </summary>
        bool IsSystemBusy { get; }

        /// <summary>
        /// Indicates whether this is currently in a process which has a progress amount completed..
        /// </summary>
        bool IsProgressActive { get; }

        /// <summary>
        /// Sets the system busy state to true.
        /// </summary>
        void StartBusy();

        /// <summary>
        /// Sets the system busy state to false.
        /// </summary>
        void StopBusy();

        /// <summary>
        /// Sets the progress active state to true.
        /// </summary>
        void StartProgress();

        /// <summary>
        /// Sets the progress active state to false.
        /// </summary>
        void StopProgress();

        /// <summary>
        /// Reports the status to subscribers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isError">Bool indicating whether the event which caused the status update was an error.</param>
        void Report(string message, bool isError = false);

        /// <summary>
        /// Reports a progress update to subscribers.
        /// </summary>
        /// <param name="progressValue">Progress current value.</param>
        /// <param name="progressMin">Progress minimum.</param>
        /// <param name="progressMax">Progress maximum.</param>
        void Progress(double progressValue, double progressMin, double progressMax);

        /// <summary>
        /// Event raised when an error has been received.
        /// </summary>
        event EventHandler<CardMimicStatusEventArgs> StatusReported;

        /// <summary>
        /// Event raised when an error has been received.
        /// </summary>
        event EventHandler<CardMimicProgressEventArgs> ProgressReported;
    }
}
