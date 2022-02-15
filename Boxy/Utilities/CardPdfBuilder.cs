using CardMimic.Properties;
using CardMimic.Reporting;
using CardMimic.ViewModels;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        public async Task DrawImagesSingleSided(List<CardViewModel> cards, IReporter reporter)
        {
            var imageIndex = 0;
            var pageIndex = 0;
            var images = new List<XImage>();

            foreach (CardViewModel card in cards)
            {
                for (var i = 0; i < card.Quantity; i++)
                {
                    await Task.Delay(1);

                    var enc = new JpegBitmapEncoder { QualityLevel = Settings.Default.PdfJpegQuality };
                    var stream = new MemoryStream();
                    enc.Frames.Add(BitmapFrame.Create(card.FrontImage));
                    enc.Save(stream);
                    images.Add(XImage.FromStream(stream));

                    if (!card.SelectedPrinting.IsDoubleFaced)
                    {
                        continue;
                    }

                    enc = new JpegBitmapEncoder { QualityLevel = Settings.Default.PdfJpegQuality };
                    stream = new MemoryStream();
                    enc.Frames.Add(BitmapFrame.Create(card.BackImage));
                    enc.Save(stream);
                    images.Add(XImage.FromStream(stream));
                }
            }

            reporter.StartProgress();

            while (imageIndex < images.Count)
            {
                if (pageIndex >= Pages.Count)
                {
                    var newPage = new PdfPage { Size = PageSize };
                    Document.AddPage(newPage);
                    Pages.Add(new CardPage(newPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor));
                }

                CardPage page = Pages[pageIndex];
                pageIndex += 1;

                List<XImage> imagesOnPage = imageIndex + page.CardsPerPage <= images.Count
                    ? images.GetRange(imageIndex, page.CardsPerPage)
                    : images.GetRange(imageIndex, images.Count - imageIndex);
                imageIndex += page.CardsPerPage;

                reporter.Report($"Performing ancient ritual {imageIndex}/{images.Count}");
                reporter.Progress(imageIndex, 0, images.Count);

                for (var i = 0; i < imagesOnPage.Count; i++)
                {
                    int row = i / page.Columns;
                    int column = i % page.Columns;
                    await page.DrawImage(new CardPdfImage(imagesOnPage[i], row, column, true));
                }
            }

            reporter.StopProgress();
        }

        public async Task DrawImagesTwoSided(List<CardViewModel> cards, IReporter reporter)
        {
            var imageIndex = 0;
            var pageIndex = 0;

            var images = new List<XImage>();

            CardViewModel card = cards[imageIndex];
            for (var i = 0; i < card.Quantity; i++)
            {
                await Task.Delay(1);
                reporter.Report($"Performing ancient ritual {imageIndex}/{cards.Count}");

                var enc = new JpegBitmapEncoder { QualityLevel = Settings.Default.PdfJpegQuality };
                var stream = new MemoryStream();
                enc.Frames.Add(BitmapFrame.Create(card.FrontImage));
                enc.Save(stream);
                images.Add(XImage.FromStream(stream));

                if (!card.SelectedPrinting.IsDoubleFaced)
                {
                    continue;
                }

                enc = new JpegBitmapEncoder { QualityLevel = Settings.Default.PdfJpegQuality };
                stream = new MemoryStream();
                enc.Frames.Add(BitmapFrame.Create(card.BackImage));
                enc.Save(stream);
                images.Add(XImage.FromStream(stream));
            }

            while (imageIndex < images.Count)
            {
                if (pageIndex >= Pages.Count)
                {
                    var newPage = new PdfPage { Size = PageSize };
                    Document.AddPage(newPage);
                    Pages.Add(new CardPage(newPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor));
                }

                CardPage page = Pages[pageIndex];
                pageIndex += 1;

                List<XImage> imagesOnPage = imageIndex + page.CardsPerPage <= images.Count
                    ? images.GetRange(imageIndex, page.CardsPerPage)
                    : images.GetRange(imageIndex, images.Count - imageIndex);
                imageIndex += page.CardsPerPage;

                for (var i = 0; i < imagesOnPage.Count; i++)
                {
                    int row = i / page.Columns;
                    int column = i % page.Columns;
                    await page.DrawImage(new CardPdfImage(imagesOnPage[i], row, column, true));
                }
            }
        }
    }
}
