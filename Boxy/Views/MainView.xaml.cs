using CardMimic.Model;
using CardMimic.Model.ScryfallData;
using CardMimic.Mvvm;
using CardMimic.Properties;
using CardMimic.ViewModels;
using CardMimic.Views.Resources;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CardMimic.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            //var locator = new ViewModelLocator();
            //DataContext = locator.MainVM;
            InitializeComponent();

            UpgradeSettings();

            Left = Settings.Default.MainWindowLeft;
            Top = Settings.Default.MainWindowTop;
            Width = Settings.Default.MainWindowWidth;
            Height = Settings.Default.MainWindowHeight;
            WindowState = Settings.Default.MainWindowState;
            WindowFixer.SizeToFit(this);
            WindowFixer.MoveIntoView(this);

            if (Settings.Default.Column0Width < 150)
            {
                Settings.Default.Column0Width = 150;
                Settings.Default.Save();
            }
            else if (Settings.Default.Column0Width > 400)
            {
                Settings.Default.Column0Width = 400;
                Settings.Default.Save();
            }

            GridColumn0.Width = new GridLength(Settings.Default.Column0Width);
        }

        /// <summary>
        /// Override of OnClosing to write the current window position/size values to the settings.
        /// </summary>
        /// <param name="e">CancelEventArgs.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                Settings.Default.MainWindowLeft = Left;
                Settings.Default.MainWindowTop = Top;
                Settings.Default.MainWindowWidth = ActualWidth;
                Settings.Default.MainWindowHeight = ActualHeight;
                Settings.Default.MainWindowState = WindowState;
                Settings.Default.Save();
            }

            (DataContext as ViewModelBase)?.Cleanup();
            base.OnClosing(e);
        }

        /// <summary>
        /// Attempt to copy user settings from previous application version if necessary.
        /// Reset to default on failure.
        /// </summary>
        private static void UpgradeSettings()
        {
            try
            {
                if (!Settings.Default.UpdateSettings)
                {
                    return;
                }

                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }
            catch
            {
                Settings.Default.Reset();
            }
        }

        private async void ButtonBase_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;

            if (!(sender is Button))
            {
                return;
            }

            var random = new Random();

            while (e.LeftButton == MouseButtonState.Pressed)
            {
                await Task.Delay(50);
                Card card = await ScryfallService.GetRandomCard(((MainViewModel) DataContext).Reporter);
                int qty = random.Next(1, 5);
                string qtyAsString = qty == 1 ? string.Empty : $"{qty} ";
                DecklistTextBox.AppendText(qtyAsString + card.Name + "\r\n");
            }
        }

        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!(sender is GridSplitter))
            {
                return;
            }

            Settings.Default.Column0Width = GridColumn0.Width.Value;
            Settings.Default.Save();
        }
    }
}
