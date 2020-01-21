using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CellManager.Resources.Reporting
{
    public class ReporterNoLog : IReporter
    {
        /// <inheritdoc />
        public bool IsSystemBusy { get; private set; }

        /// <inheritdoc />
        public void StartBusy()
        {
            IsSystemBusy = true;
            OnPropertyChanged(nameof(IsSystemBusy));
        }

        /// <inheritdoc />
        public void StopBusy()
        {
            IsSystemBusy = false;
            OnPropertyChanged(nameof(IsSystemBusy));
        }

        /// <inheritdoc />
        public void Report(string value)
        {
            StatusReported?.Invoke(this, value);
        }

        /// <inheritdoc />
        public void ReportError(object sender, Exception exc, bool suppressDisplay = false)
        {
            if (exc == null)
            {
                throw new ArgumentNullException(nameof(exc));
            }

            ErrorReported?.Invoke(sender, new CellManagerErrorEventArgs(exc, string.Empty, suppressDisplay));
        }

        /// <inheritdoc />
        public void ReportError(object sender, string message, bool suppressDisplay = false)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ErrorReported?.Invoke(sender, new CellManagerErrorEventArgs(null, message, suppressDisplay));
        }

        /// <inheritdoc />
        public void ReportError(object sender, Exception exc, string message, bool suppressDisplay = false)
        {
            if (exc == null)
            {
                throw new ArgumentNullException(nameof(exc));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ErrorReported?.Invoke(sender, new CellManagerErrorEventArgs(exc, message, suppressDisplay));
        }

        /// <inheritdoc />
        public event EventHandler<CellManagerErrorEventArgs> ErrorReported;

        /// <summary>
        /// Event handler for property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public event EventHandler<string> StatusReported;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
