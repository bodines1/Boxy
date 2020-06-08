using System.Windows;

namespace CardMimic.Views.Resources
{
    /// <summary>
    /// Has methods for fixing the layout of a window if it was, for example, closed off screen.
    /// </summary>
    public static class WindowFixer
    {
        /// <summary>
        /// Sizes a window to fit the current desktop if it is too large.
        /// </summary>
        /// <param name="window">Window to manipulate.</param>
        public static void SizeToFit(Window window)
        {
            if (window.Height > SystemParameters.VirtualScreenHeight)
            {
                window.Height = SystemParameters.VirtualScreenHeight;
            }

            if (window.Width > SystemParameters.VirtualScreenWidth)
            {
                window.Width = SystemParameters.VirtualScreenWidth;
            }
        }

        /// <summary>
        /// Moves a window back onto the desktop if it is too far out of bounds.
        /// </summary>
        /// <param name="window">Window to manipulate.</param>
        public static void MoveIntoView(Window window)
        {
            if (window.Top + window.Height / 2 > SystemParameters.VirtualScreenHeight)
            {
                window.Top = SystemParameters.VirtualScreenHeight - window.Height;
            }

            if (window.Left + window.Width / 2 > SystemParameters.VirtualScreenWidth)
            {
                window.Left = SystemParameters.VirtualScreenWidth - window.Width;
            }

            if (window.Top < 0)
            {
                window.Top = 0;
            }

            if (window.Left < 0)
            {
                window.Left = 0;
            }
        }
    }
}
