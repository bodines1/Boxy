using Boxy.Model.ScryfallData;
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

        public static bool IsCacheBeingAccessed { get; set; }

        /// <summary>
        /// Gets the cached bitmap image representing the card object. Will query the API if it has not been loaded, otherwise gets the cached version.
        /// </summary>
        public static async Task<Bitmap> GetImageAsync(Card card, IProgress<string> reporter)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), @"Card object cannot be null. Consumer must check object before using this method.");
            }

            while (IsCacheBeingAccessed)
            {
                await Task.Delay(1);
            }

            IsCacheBeingAccessed = true;

            if (ImageCache.ContainsKey(card.Id))
            {
                await Task.Delay(1);
                IsCacheBeingAccessed = false;
                return ImageCache[card.Id];
            }

            Bitmap bitmap = await ScryfallService.GetBorderCropImageAsync(card, reporter);
            ImageCache.Add(card.Id, bitmap);

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
