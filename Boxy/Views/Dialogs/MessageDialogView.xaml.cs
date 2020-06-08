using CardMimic.DialogService;
using CardMimic.Mvvm;
using System.ComponentModel;

namespace CardMimic.Views.Dialogs
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
