using Boxy.DialogService;
using Boxy.Mvvm;
using System.ComponentModel;

namespace Boxy.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for YesNoDialogView.xaml
    /// </summary>
    public partial class YesNoDialogView : IDialog
    {
        public YesNoDialogView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as ViewModelBase)?.Cleanup();
            base.OnClosing(e);
        }
    }
}
