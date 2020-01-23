using Boxy.Mvvm;
using Boxy.Properties;
using Boxy.Views.Resources;
using System.ComponentModel;
using System.Windows;

namespace Boxy.Views
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

            // So I don't have to type it a bunch during testing.
            SubmitTextBox.Text = "Time Wipe\r\nAgent of Treachery\r\nArchon of Sun's Grace\r\nBreeding Pool\r\nCavalier of Dawn\r\nCavalier of Gales\r\nDeputy of Detention\r\nDream Trawler\r\nDryad of the Ilysian Grove\r\nElite Guardmage\r\nElspeth Conquers Death\r\nEnigmatic Incarnation\r\nFabled Passage\r\nGolos, Tireless Pilgrim\r\nHallowed Fountain\r\nKnight of Autumn\r\nOmen of the Hunt\r\nOmen of the Sea\r\nSetessan Champion\r\nSpark Double\r\nTemple Garden\r\nTemple of Enlightenment\r\nThassa, Deep-Dwelling\r\nTolsimir, Friend to Wolves\r\nWolfwillow Haven";
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
    }
}
