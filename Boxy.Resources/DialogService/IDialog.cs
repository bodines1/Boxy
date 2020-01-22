namespace Boxy.Resources.DialogService
{
    /// <summary>
    /// Dialog view models.
    /// </summary>
    public interface IDialog
    {
        /// <summary>
        /// Dialog viewmodel.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        /// Result from the user.
        /// </summary>
        bool? DialogResult { get; set; }

        /// <summary>
        /// Close the dialog.
        /// </summary>
        void Close();

        /// <summary>
        /// Shows the view and returns the result.
        /// </summary>
        /// <returns>True or false depending on user interaction. Null if window closed.</returns>
        bool? ShowDialog();

        /// <summary>
        /// Shows the view.
        /// </summary>
        void Show();
    }
}
