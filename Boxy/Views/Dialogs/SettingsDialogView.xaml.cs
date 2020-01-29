﻿using Boxy.DialogService;
using Boxy.Mvvm;
using System.ComponentModel;

namespace Boxy.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialogView.xaml
    /// </summary>
    public partial class SettingsDialogView : IDialog
    {
        public SettingsDialogView()
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
