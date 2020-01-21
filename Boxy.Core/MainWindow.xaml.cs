using Boxy.Model;
using System.Windows;
using System.Windows.Documents;

namespace Boxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            SubmitTextBox.Document.Blocks.Clear();
            SubmitTextBox.Document.Blocks.Add(new Paragraph(new Run("Merfolk Secretkeeper")));
        }

        private async void SubmitButton_OnClick(object sender, RoutedEventArgs e)
        {
            BusyBorder.Visibility = Visibility.Visible;
            var text = new TextRange(SubmitTextBox.Document.ContentStart, SubmitTextBox.Document.ContentEnd).Text; 
            var card = await ScryfallService.GetCardsAsync(text);
            var bitmap = await ImageCaching.GetImageAsync(card);
            ImageDisplay.Source = ImageHelper.LoadBitmap(bitmap);
            BusyBorder.Visibility = Visibility.Collapsed;
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            ImageDisplay.Source = null;
        }
    }
}
