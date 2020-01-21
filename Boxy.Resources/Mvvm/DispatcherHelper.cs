using System.Windows.Threading;

namespace CellManager.Resources.MvvmUtilities
{
    /// <summary>
    /// Has tools to help access the UI dispatcher from other threads.
    /// </summary>
    public static class DispatcherHelper
    {
        /// <summary>
        /// The main UI dispatcher. Will be null until <see cref="Initialize"/> is called.
        /// </summary>
        public static Dispatcher UiDispatcher { get; private set; }

        /// <summary>
        /// Call this method once on application startup from the main UI thread. Allows accessing <see cref="UiDispatcher"/> later in application lifetime.
        /// </summary>
        public static void Initialize()
        {
            UiDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
