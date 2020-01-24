using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boxy.Utilities
{
    public class CardPdfBuilder2
    {
        public CardPdfBuilder2(PageSize pageSize, double scaling, bool hasCutLines)
        {
            Document = new PdfDocument
            {
                Info =
                {
                    Title = "MTG Proxies by Boxy",
                    Author = "Sean Bodine",
                    CreationDate = DateTime.Now,
                    Creator = "Boxy",
                    Subject = "MTG Proxies",
                }
            };

            var firstPage = new PdfPage { Size = pageSize };
            Document.AddPage(firstPage);
            Pages.Add(new CardPage(firstPage, scaling, hasCutLines));
        }

        /// <summary>
        /// Document object which holds all the pdf pages as they get created.
        /// </summary>
        public PdfDocument Document { get; }

        private List<CardPage> Pages { get; }

        public async Task DrawImages(List<XImage> images)
        {
            var imageIndex = 0;
            var pageIndex = 0;

            while (imageIndex < images.Count)
            {
                CardPage page = Pages[pageIndex];
                await page.DrawImages(images.GetRange(imageIndex, page.CardsPerPage));
            }

            
        }
    }
}
