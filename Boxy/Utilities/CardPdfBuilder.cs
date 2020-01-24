using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Boxy.Utilities
{
    public class CardPdfBuilder
    {
        public CardPdfBuilder(PageSize pageSize, double scaling, bool hasCutLines)
        {
            PageSize = pageSize;
            GutterThickness = hasCutLines ? 1 : 0;
            Document = new PdfDocument
            {
                Info =
                {
                    Title = "MTG Proxies by Boxy",
                    Author = "Sean Bodine",
                    CreationDate = DateTime.Now,
                    Creator = "Boxy",
                    Subject = "MTG Proxies"
                }
            };

            Page = AddPage();

            // MTG cards are 3.48 x 2.49 inches or 63 x 88 mm
            CardSize = new XSize(2.49 * PointsPerInch * scaling, 3.48 * PointsPerInch * scaling);

            if (CardSize.Width + Margin > PageXLimit || CardSize.Height + Margin > PageYLimit)
            {
                throw new InvalidOperationException($"Page {pageSize} is too small for the scaling {scaling * 100}% being used, no cards will fit on a page. Please use a different paper size or a smaller scaling.");
            }
        }

        /// <summary>
        /// Document object which holds all the pdf pages as they get created.
        /// </summary>
        public PdfDocument Document { get; }

        /// <summary>
        /// What page the builder is currently on.
        /// </summary>
        public int Page { get; private set; }

        /// <summary>
        /// What row the builder is currently on.
        /// </summary>
        private int Row { get; set; }

        /// <summary>
        /// What column the builder is currently on.
        /// </summary>
        private int Column { get; set; }
        
        private XGraphics Gfx { get; set; }

        private PageSize PageSize { get; }

        private double PointsPerInch { get; set; }

        private double GutterThickness { get; }

        private double Margin { get; set; }

        private double PageXLimit { get; set; }

        private double PageYLimit { get; set; }

        private XSize CardSize { get; }

        private int AddPage()
        {
            // Add the page and make a graphics drawer.
            var page = new PdfPage { Size = PageSize };
            Document.AddPage(page);
            Gfx = XGraphics.FromPdfPage(page);

            // Set some properties other methods will need to use.
            PointsPerInch = page.Width.Point / page.Width.Inch;
            Margin = 0.25 * PointsPerInch;
            PageXLimit = page.Width - Margin;
            PageYLimit = page.Height - Margin;

            // Create a font.
            var font = new XFont(FontFamily.GenericMonospace, 20, XFontStyle.Regular);

            // Draw the text.
            Gfx.DrawString("Proxies by Boxy", font, XBrushes.AntiqueWhite, new XRect(0, -4, page.Width, page.Height), XStringFormats.TopCenter);
            Gfx.DrawString("Proxies by Boxy", font, XBrushes.AntiqueWhite, new XRect(0, 0, page.Width, page.Height + 2), XStringFormats.BottomCenter);

            return Document.PageCount - 1;
        }

        private bool PredictXInBounds()
        {
            double xPos = Margin + Column * (CardSize.Width + GutterThickness);
            return xPos + CardSize.Width + GutterThickness < PageXLimit;
        }

        private bool PredictYInBounds()
        {
            double yPos = Margin + Row * (CardSize.Height + GutterThickness);
            return yPos + CardSize.Height + GutterThickness < PageYLimit;
        }

        private XRect FindImagePlacement()
        {
            // If the current working page is more than the available pages, add a new page.
            if (Page > Document.PageCount - 1)
            {
                Page = AddPage();
            }

            // Calculate the position of the top left corner of the image.
            double xPos = Margin + Column * (CardSize.Width + GutterThickness);
            double yPos = Margin + Row * (CardSize.Height + GutterThickness);
            var position = new XPoint(xPos, yPos);
            var imagePlacement = new XRect(position, CardSize);
            return imagePlacement;
        }

        private void DrawImage(Stream memoryStream, XRect imagePlacement)
        {
            XImage xImage = XImage.FromStream(memoryStream);
            Gfx.DrawImage(xImage, imagePlacement);
        }

        private void SetNextPositions()
        {
            Column += 1;

            if (PredictXInBounds())
            {
                return;
            }

            Row += 1;
            Column = 0;

            if (PredictYInBounds())
            {
                return;
            }

            Row = 0;
            Page += 1;
        }

        private void AddImage(Stream imageStream)
        {
            XRect imagePlacement = FindImagePlacement();
            DrawImage(imageStream, imagePlacement);
            SetNextPositions();
        }

        public async Task AddImageAsync(Stream imageStream)
        {
            await Task.Run(() => AddImage(imageStream));
        }
    }
}
