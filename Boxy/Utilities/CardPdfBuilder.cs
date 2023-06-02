using CardMimic.Reporting;
using CardMimic.ViewModels;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardMimic.Utilities
{
    /// <summary>
    /// Builds PDF objects from a set of card view model objects.
    /// </summary>
    public class CardPdfBuilder
    {
        /// <summary>
        /// Creates a new instance of <see cref="CardPdfBuilder"/>.
        /// </summary>
        public CardPdfBuilder(PageSize pageSize, double scalingPercent, bool hasCutLines, CutLineSizes cutLineSize, XKnownColor cutLineColor)
        {
            PageSize = pageSize;
            ScalingPercent = scalingPercent;
            HasCutLines = hasCutLines;
            CutLineSize = cutLineSize;
            CutLineColor = cutLineColor;
            var exampleDoc = new PdfDocument();
            var examplePage = new PdfPage { Size = PageSize };
            exampleDoc.AddPage(examplePage);
            ExampleImageDrawer = new CardImageDrawer(examplePage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor);
        }

        private PageSize PageSize { get; }

        private double ScalingPercent { get; }

        private bool HasCutLines { get; }

        private CutLineSizes CutLineSize { get; }

        private XKnownColor CutLineColor { get; }

        /// <summary>
        /// And example page that can be used for layout prediction, etc.
        /// </summary>
        public CardImageDrawer ExampleImageDrawer { get; }

        #region Methods

        private static List<CardPageImage> ExpandSingleSided(IEnumerable<CardViewModel> cards)
        {
            var result = new List<CardPageImage>();

            foreach (CardViewModel card in cards)
            {
                for (var i = 0; i < card.Quantity; i++)
                {
                    result.Add(card.GetFrontCardPageImage());

                    if (card.BackImage != null)
                    {
                        result.Add(card.GetBackCardPageImage());
                    }
                }
            }

            return result;
        }

        private static List<CardPageImage> ExpandDoubleSided(IEnumerable<CardViewModel> cards)
        {
            var result = new List<CardPageImage>();

            foreach (CardViewModel card in cards)
            {
                for (var i = 0; i < card.Quantity; i++)
                {
                    result.Add(card.GetFullCardPageImage());
                }
            }

            return result;
        }

        public async Task<PdfDocument> BuildPdfSingleSided(IEnumerable<CardViewModel> cards, IReporter reporter)
        {
            var document = new PdfDocument
            {
                Info =
                {
                    Title = "Card Mimic",
                    Author = "Sean Bodine",
                    CreationDate = DateTime.Now,
                    Creator = "Card Mimic",
                    Subject = "Proxies",
                }
            };

            List<CardPageImage> expandedCards = ExpandSingleSided(cards);
            var cardIndex = 0;
            reporter.StartProgress();

            while (cardIndex < expandedCards.Count)
            {
                var frontPdfPage = new PdfPage { Size = PageSize };
                document.AddPage(frontPdfPage);
                var imageDrawer = new CardImageDrawer(frontPdfPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor);

                int cardsPerPage = imageDrawer.ImagesPerPage;

                List<CardPageImage> imagesOnPage = cardIndex + cardsPerPage <= expandedCards.Count
                    ? expandedCards.GetRange(cardIndex, cardsPerPage)
                    : expandedCards.GetRange(cardIndex, expandedCards.Count - cardIndex);

                cardIndex += cardsPerPage;
                reporter.Progress(cardIndex, 0, expandedCards.Count);

                for (var i = 0; i < imagesOnPage.Count; i++)
                {
                    CardPageImage cardPageImage = imagesOnPage[i];
                    int row = i / imageDrawer.Columns;
                    int frontColumn = i % imageDrawer.Columns;

                    await imageDrawer.DrawImage(cardPageImage.FrontImage, row, frontColumn);
                }
            }

            reporter.StopProgress();
            return document;
        }

        public async Task<PdfDocument> BuildPdfTwoSided(IEnumerable<CardViewModel> cards, IReporter reporter)
        {
            var document = new PdfDocument
            {
                Info =
                {
                    Title = "Card Mimic",
                    Author = "Sean Bodine",
                    CreationDate = DateTime.Now,
                    Creator = "Card Mimic",
                    Subject = "Proxies",
                }
            };

            List<CardPageImage> expandedCards = ExpandDoubleSided(cards);
            var cardIndex = 0;
            reporter.StartProgress();

            while (cardIndex < expandedCards.Count)
            {
                var frontPdfPage = new PdfPage { Size = PageSize };
                document.AddPage(frontPdfPage);
                var frontImageDrawer = new CardImageDrawer(frontPdfPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor);

                var backPdfPage = new PdfPage { Size = PageSize };
                document.AddPage(backPdfPage);
                var backImageDrawer = new CardImageDrawer(backPdfPage, ScalingPercent, HasCutLines, CutLineSize, CutLineColor);

                int cardsPerPage = ExampleImageDrawer.ImagesPerPage;

                List<CardPageImage> imagesOnPage = cardIndex + cardsPerPage <= expandedCards.Count
                    ? expandedCards.GetRange(cardIndex, cardsPerPage)
                    : expandedCards.GetRange(cardIndex, expandedCards.Count - cardIndex);

                cardIndex += cardsPerPage;
                reporter.Progress(cardIndex, 0, expandedCards.Count);

                for (var i = 0; i < imagesOnPage.Count; i++)
                {
                    CardPageImage cardPageImage = imagesOnPage[i];
                    int row = i / frontImageDrawer.Columns;
                    int frontColumn = i % frontImageDrawer.Columns;
                    int backColumn = frontImageDrawer.Columns - 1 - frontColumn;

                    await frontImageDrawer.DrawImage(cardPageImage.FrontImage, row, frontColumn);

                    if (cardPageImage.BackImage != null)
                    {
                        await backImageDrawer.DrawImage(cardPageImage.BackImage, row, backColumn);
                    }
                }
            }

            reporter.StopProgress();
            return document;
        }

        #endregion Methods
    }
}
