using Boxy.Model;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

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
            SubmitTextBox.Text = "Merfolk Secretkeeper";
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(Bitmap source)
        {
            var ip = source.GetHbitmap();
            BitmapSource bs;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        private void SubmitButton_OnClick(object sender, RoutedEventArgs e)
        {
            var card = ScryfallService.GetCards(SubmitTextBox.Text.Trim());
            var bitmap = ImageCaching.GetImage(card);
            ImageDisplay.Source = LoadBitmap(bitmap);
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            ImageDisplay.Source = null;
        }
    }
}
