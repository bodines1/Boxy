using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Boxy.Resources.DialogService
{
    /// <summary>
    /// Concrete implementation of <see cref="IDialogService"/>.
    /// </summary>
    public class DialogService : IDialogService
    {
        #region Properties

        /// <summary>
        /// Holds the v/vm pairs.
        /// </summary>
        private IDictionary<Type, Type> Mappings { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="DialogService"/>.
        /// </summary>
        public DialogService()
        {
            Mappings = new Dictionary<Type, Type>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Register the view / viewmodel pairs for reference.
        /// </summary>
        /// <typeparam name="TViewModel">The datacontext for this viewmodel.</typeparam>
        /// <typeparam name="TView">The view associated with the datacontext.</typeparam>
        public void Register<TViewModel, TView>()
            where TViewModel : IDialogRequestClose
            where TView : IDialog
        {
            if (Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($@"Type {typeof(TViewModel)} is already mapped to type {typeof(TView)}", nameof(TViewModel));
            }

            Mappings.Add(typeof(TViewModel), typeof(TView));
        }

        /// <summary>
        /// Modal dialog show.
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel used for the datacontext.</typeparam>
        /// <param name="viewModel">Type of viewmodel to look up.</param>
        /// <returns>A dialog result.</returns>
        public bool? ShowDialog<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogRequestClose
        {
            if (!Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($@"View model type {typeof(TViewModel)} has no mapping to a view. Please ensure any view model type being called has been registered with the dialog service.", nameof(viewModel));
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            Type viewType = Mappings[typeof(TViewModel)];
            var dialog = (IDialog)Activator.CreateInstance(viewType);

            // needs access to local variables.
            void EventHandler(object sender, DialogCloseRequestedEventArgs e)
            {
                dispatcher.Invoke(() => // <---- Here.
                {
                    viewModel.CloseRequested -= EventHandler; // <---- Here.

                    if (e.DialogResult.HasValue)
                    {
                        dialog.DialogResult = e.DialogResult; // <---- Here.
                    }
                    else
                    {
                        dialog.Close();
                    }
                });
            }

            viewModel.CloseRequested += EventHandler;
            dialog.DataContext = viewModel;
            return dialog.ShowDialog();
        }

        /// <summary>
        /// Non-modal dialog show.
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel used for the datacontext.</typeparam>
        /// <param name="viewModel">Type of viewmodel to look up.</param>
        /// <returns>A dialog result.</returns>
        public void Show<TViewModel>(TViewModel viewModel)
            where TViewModel : IDialogRequestClose
        {
            if (!Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($@"View model type {typeof(TViewModel)} has no mapping to a view. Please ensure any view model type being called has been registered with the dialog service.", nameof(viewModel));
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            Type viewType = Mappings[typeof(TViewModel)];
            var dialog = (IDialog)Activator.CreateInstance(viewType);

            // needs access to local variables.
            void EventHandler(object sender, DialogCloseRequestedEventArgs e)
            {
                dispatcher.Invoke(() => // <---- Here.
                {
                    viewModel.CloseRequested -= EventHandler; // <---- Here.
                    dialog.Close();
                });
            }

            viewModel.CloseRequested += EventHandler;
            dialog.DataContext = viewModel;
            dialog.Show();
        }

        #endregion Methods
    }
}
