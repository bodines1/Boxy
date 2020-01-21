using Boxy.DialogService;
using Boxy.IoC;
using Boxy.ViewModels;
using Unity;

namespace Boxy.Views.Resources
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocator"/> class.
        /// </summary>
        public ViewModelLocator()
        {
            // Create the containers.
            var container = new UnityContainer();
            var dialogService = new DialogService.DialogService();
            _resolver = new UnityDependencyResolver(container);

            // Register the other IoC objects with the container.
            container.RegisterType<IDialogService, DialogService.DialogService>();
            container.RegisterInstance(dialogService);
            container.RegisterType<IDependencyResolver, UnityDependencyResolver>();
            container.RegisterInstance(_resolver);

            // Initialize container dependencies.
            InitializeDialogService(dialogService);
        }

        private readonly IDependencyResolver _resolver;
        private MainViewModel _mainViewModel;

        /// <summary>
        /// Gets a new OperationTasksMainViewModel.
        /// </summary>
        public MainViewModel MainVM
        {
            get
            {
                return _mainViewModel ?? (_mainViewModel = _resolver.Resolve<MainViewModel>());
            }
        }

        private static void InitializeDialogService(IDialogService dialogService)
        {
            //dialogService.Register<InfoMessageDialogViewModel, InfoMessageDialogView>();
            //dialogService.Register<YesNoMessageViewModel, YesNoMessageView>();
            //dialogService.Register<ErrorMessageDialogViewModel, ErrorMessageDialogView>();
        }
    }
}
