using Boxy.DialogService;
using Boxy.Mvvm;
using System.ComponentModel;

namespace Boxy.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageDialogView.xaml
    /// </summary>
    public partial class MessageDialogView : IDialog
    {
        public MessageDialogView()
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
