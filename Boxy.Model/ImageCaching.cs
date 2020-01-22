using Boxy.Model.ScryfallData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Boxy.Model
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

        /// <summary>
        /// Gets the cached bitmap image representing the card object. Will query the API if it has not been loaded, otherwise gets the cached version.
        /// </summary>
        public static async Task<Bitmap> GetImageAsync(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card), "Card object cannot be null. Consumer must check object before using this method.");
            }

            if (ImageCache.ContainsKey(card.Id))
            {
                return ImageCache[card.Id];
            }

            Bitmap bitmap = await ScryfallService.GetBorderCropImageAsync(card);
            ImageCache.Add(card.Id, bitmap);

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
