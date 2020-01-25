using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Boxy.Model.SerializedData
{
    /// <summary>
    /// Class for accessing bitmap objects pulled from the API, to avoid re-querying for them.
    /// </summary>
    public static class ImageCaching
    {
        private static Dictionary<string, Bitmap> _imageCache;

        private static Dictionary<string, Bitmap> ImageCache
        {
            get
            {
                return _imageCache ?? (_imageCache = new Dictionary<string, Bitmap>());
            }
        }

        private static bool IsCacheBeingAccessed { get; set; }

        /// <summary>
        /// Gets the cached bitmap image representing the card object. Will query the API if it has not been loaded, otherwise gets the cached version.
        /// </summary>
        public static async Task<Bitmap> GetImageAsync(string imageUri, IProgress<string> reporter)
        {
            if (string.IsNullOrWhiteSpace(imageUri))
            {
                throw new ArgumentNullException(nameof(imageUri), @"Image request URI cannot be null or empty/whitespace. Consumer must check before using this method.");
            }

            while (IsCacheBeingAccessed)
            {
                await Task.Delay(1);
            }

            IsCacheBeingAccessed = true;

            if (ImageCache.ContainsKey(imageUri))
            {
                await Task.Delay(1);
                IsCacheBeingAccessed = false;
                return ImageCache[imageUri];
            }

            Bitmap bitmap = await ScryfallService.GetImageAsync(imageUri, reporter);
            ImageCache.Add(imageUri, bitmap);

            if (ImageCache.Count > 100)
            {
                ImageCache.Remove(ImageCache.First().Key);
            }
            
            IsCacheBeingAccessed = false;
            return bitmap;
        }

        /// <summary>
        /// Clears the cache to ensure the images are released from memory correctly.
        /// </summary>
        public static void Clear()
        {
            ImageCache.Clear();
        }
    }
}
