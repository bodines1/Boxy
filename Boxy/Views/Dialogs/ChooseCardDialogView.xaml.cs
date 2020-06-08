using CardMimic.DialogService;
using CardMimic.Mvvm;
using System.ComponentModel;

namespace CardMimic.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ChooseCardDialogView.xaml
    /// </summary>
    public partial class ChooseCardDialogView : IDialog
    {
        public ChooseCardDialogView()
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
