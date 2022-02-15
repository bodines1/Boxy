using CardMimic.Reporting;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardMimic.Utilities
{
    public class CardPdfBuilder
    {
        public CardPdfBuilder(PageSize pageSize, double scalingPercent, bool hasCutLines, CutLineSizes cutLineSize, XKnownColor cutLineColor)
        {
            PageSize = pageSize;
            ScalingPercent = scalingPercent;
            HasCutLines = hasCutLines;
            CutLineSize = cutLineSize;
            CutLineColor = cutLineColor;

            Document = new PdfDocument
            {
                Info =
                {
                    Title = "Card Mimic",
                    Author = "Sean Bodine",
                    CreationDate = DateTime.Now,
                    Creator = "Card Mimic",
                    Subject = "MTG Proxies",
                }
            };

            var firstPage = new PdfPage { Size = PageSize };
            Document.AddPage(firstPage);
            Pages = new List<CardPage> { new CardPage(firstPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor) };
        }

        private PageSize PageSize { get; }

        private double ScalingPercent { get; }

        private bool HasCutLines { get; }

        private CutLineSizes CutLineSize { get; }

        private XKnownColor CutLineColor { get; }

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
                    Pages.Add(new CardPage(newPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor));
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
