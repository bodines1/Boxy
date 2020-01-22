namespace Boxy.Resources.DialogService
{
    /// <summary>
    /// Interface for implementing a service which displays and handles dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Register a View and ViewModel pair to display a dialog.
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel to pair.</typeparam>
        /// <typeparam name="TView">View to pair.</typeparam>
        void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog;

        /// <summary>
        /// Shows the modal view for the specific dialog viewmodel.
        /// </summary>
        /// <typeparam name="TViewModel">Type of ViewModel implementing IDialogRequestClose.</typeparam>
        /// <param name="viewModel">New viewmodel as the datacontext.</param>
        /// <returns>Dialog result.</returns>
        bool? ShowDialog<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogRequestClose;

        /// <summary>
        /// Shows the non-modal view for the specific dialog viewmodel.
        /// </summary>
        /// <typeparam name="TViewModel">Type of ViewModel implementing IDialogRequestClose.</typeparam>
        /// <param name="viewModel">New ViewModel as the datacontext.</param>
        void Show<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogRequestClose;
    }
}
