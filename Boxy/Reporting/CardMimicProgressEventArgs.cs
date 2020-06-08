using System;

namespace CardMimic.Reporting
{
    /// <summary>
    /// Event args to report a status message to a listener.
    /// </summary>
    public class CardMimicProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the EventArgs.
        /// </summary>
        /// <param name="progressValue">Progress current value.</param>
        /// <param name="progressMin">Progress minimum.</param>
        /// <param name="progressMax">Progress maximum.</param>
        public CardMimicProgressEventArgs(double progressValue, double progressMin, double progressMax)
        {
            ProgressValue = progressValue;
            ProgressMin = progressMin;
            ProgressMax = progressMax;
        }

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
