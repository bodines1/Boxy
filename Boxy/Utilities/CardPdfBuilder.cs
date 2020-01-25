using Boxy.Reporting;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boxy.Utilities
{
    public class CardPdfBuilder
    {
        public CardPdfBuilder(PageSize pageSize, double scaling, bool hasCutLines)
        {
            PageSize = pageSize;
            Scaling = scaling;
            HasCutLines = hasCutLines;

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
            Pages = new List<CardPage> { new CardPage(firstPage, scaling, hasCutLines) };
        }

        private PageSize PageSize { get; }

        private double Scaling { get; }

        private bool HasCutLines { get; }

        /// <summary>
        /// Card page objects created so far. Has at least one immediately after construction.
        /// </summary>
        public List<CardPage> Pages { get; }

        /// <summary>
        /// Document object which holds all the pdf pages as they get created.
        /// </summary>
        public PdfDocument Document { get; }

        
        public async Task DrawImages(List<XImage> images, IReporter reporter)
        {
            var imageIndex = 0;
            var pageIndex = 0;

            while (imageIndex < images.Count)
            {
                if (pageIndex >= Pages.Count)
                {
                    var newPage = new PdfPage { Size = PageSize };
                    Document.AddPage(newPage);
                    Pages.Add(new CardPage(newPage, Scaling, HasCutLines));
                }

                CardPage page = Pages[pageIndex];
                reporter.Report($"Inscribing the codex with runes, Page {pageIndex}");

                List<XImage> imageSubset = imageIndex + page.CardsPerPage <= images.Count
                    ? images.GetRange(imageIndex, page.CardsPerPage)
                    : images.GetRange(imageIndex, images.Count - imageIndex);

                await page.DrawImages(imageSubset);

                imageIndex += page.CardsPerPage;
                pageIndex += 1;
            }

            
        }
    }
}
