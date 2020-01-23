using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Boxy.Utilities
{
    public static class ImageHelper
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
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

        public static async Task<BitmapSource> LoadBitmapAsync(Bitmap source)
        {
            return await Task.Run(() => LoadBitmap(source));
        }
    }
}
