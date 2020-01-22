using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Boxy.Resources.Reporting
{
    public class ReporterNoLog : IReporter
    {
        /// <inheritdoc />
        public bool IsSystemBusy { get; private set; }

        /// <inheritdoc />
        public bool IsProgressActive { get; private set; }

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
        public void StartProgress()
        {
            IsProgressActive = true;
            OnPropertyChanged(nameof(IsSystemBusy));
        }

        /// <inheritdoc />
        public void StopProgress()
        {
            IsProgressActive = false;
            OnPropertyChanged(nameof(IsSystemBusy));
        }

        /// <inheritdoc />
        public void Report(string value)
        {
            StatusReported?.Invoke(this, new BoxyStatusEventArgs(this, value, false));
        }

        /// <inheritdoc />
        public void Report(object sender, string message, bool isError = false)
        {
            StatusReported?.Invoke(this, new BoxyStatusEventArgs(sender, message, isError));
        }

        /// <inheritdoc />
        public void Progress(object sender, double progressValue, double progressMin, double progressMax)
        {
            ProgressReported?.Invoke(this, new BoxyProgressEventArgs(sender, progressValue, progressMin, progressMax));
        }

        /// <summary>
        /// Event handler for property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public event EventHandler<BoxyStatusEventArgs> StatusReported;

        /// <inheritdoc />
        public event EventHandler<BoxyProgressEventArgs> ProgressReported;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
