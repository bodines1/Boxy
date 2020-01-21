using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Boxy.Reporting
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
            StatusReported?.Invoke(this, new BoxyStatusEventArgs(this, value, false));
        }

        /// <inheritdoc />
        public void Report(object sender, string message, bool isError)
        {
            StatusReported?.Invoke(this, new BoxyStatusEventArgs(sender, message, isError));
        }

        /// <summary>
        /// Event handler for property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public event EventHandler<BoxyStatusEventArgs> StatusReported;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
