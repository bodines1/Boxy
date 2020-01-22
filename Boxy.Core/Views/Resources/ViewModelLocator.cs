using Boxy.Resources.DialogService;
using Boxy.Resources.Reporting;
using Boxy.ViewModels;
using Boxy.ViewModels.Dialogs;
using Boxy.Views.Dialogs;

namespace Boxy.Views.Resources
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocator"/> class.
        /// </summary>
        public ViewModelLocator()
        {
            // Initialize other
            DialogService = new DialogService();
            Reporter = new ReporterNoLog();

            // Initialize container dependencies.
            DialogService.Register<MessageDialogViewModel, MessageDialogView>();

            // Initialize View Models
            MainVM = new MainViewModel(DialogService, Reporter);
            
        }

        #endregion Constructors

        #region Dependencies

        private DialogService DialogService { get; }

        private IReporter Reporter { get; }

        #endregion Dependencies

        #region View First ViewModels

        /// <summary>
        /// Gets a new OperationTasksMainViewModel.
        /// </summary>
        public MainViewModel MainVM { get; }

        #endregion View First ViewModels
    }
}
