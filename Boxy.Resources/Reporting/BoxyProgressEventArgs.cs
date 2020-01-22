using System;

namespace Boxy.Resources.Reporting
{
    /// <summary>
    /// Event args to report a status message to a listener.
    /// </summary>
    public class BoxyProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the EventArgs.
        /// </summary>
        /// <param name="sender">Class which originated the progress update call.</param>
        /// <param name="progressValue">Progress current value.</param>
        /// <param name="progressMin">Progress minimum.</param>
        /// <param name="progressMax">Progress maximum.</param>
        public BoxyProgressEventArgs(object sender, double progressValue, double progressMin, double progressMax)
        {
            Sender = sender;
            ProgressValue = progressValue;
            ProgressMin = progressMin;
            ProgressMax = progressMax;
        }

        /// <summary>
        /// Class which originated the progress update call.
        /// </summary>
        public object Sender { get; }

        /// <summary>
        /// Progress current value.
        /// </summary>
        public double ProgressValue { get; }

        /// <summary>
        /// Progress minimum.
        /// </summary>
        public double ProgressMin { get; }

        /// <summary>
        /// Progress maximum.
        /// </summary>
        public double ProgressMax { get; }
    }
}
