using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Boxy.Utilities
{
    public class CardPage 
    {
        public CardPage(PdfPage page, double scaling, bool hasCutLines)
        {
            Gfx = XGraphics.FromPdfPage(page);

            // Set Gutter
            GutterThickness = hasCutLines ? 0.666 : 0;

            // Create a font.
            MarginFont = new XFont(FontFamily.GenericMonospace, 20, XFontStyle.Regular);

            // Set some properties other methods will need to use.
            PointsPerInch = page.Width.Point / page.Width.Inch;
            Margin = 0.25 * PointsPerInch;
            UseableX = page.Width - 2 * Margin;
            UseableY = page.Height - 2 * Margin;

            // MTG cards are 3.48 x 2.49 inches or 63 x 88 mm, then slightly scaled down to fit better in card sleeves.
            CardSize = new XSize(2.49 * PointsPerInch * scaling * 0.99, 3.48 * PointsPerInch * scaling * 0.99);

            // Predict the number of cards per row and cards per column
            Rows = (int)(UseableY / CardSize.Height);
            Columns = (int)(UseableX / CardSize.Width);
            CardsPerPage = Rows * Columns;

            // Draw watermark
            Gfx.DrawString("Proxies by Boxy", MarginFont, XBrushes.AntiqueWhite, new XRect(0, -4, page.Width, page.Height), XStringFormats.TopCenter);
            Gfx.DrawString("Proxies by Boxy", MarginFont, XBrushes.AntiqueWhite, new XRect(0, 0, page.Width, page.Height + 2), XStringFormats.BottomCenter);

            if (hasCutLines)
            {
                Gfx.DrawRectangle(XBrushes.Gray, Margin, Margin, (CardSize.Width + GutterThickness) * Columns, (CardSize.Height + GutterThickness) * Rows);
            }
        }

        private XSize CardSize { get; }

        private XGraphics Gfx { get; }

        private double GutterThickness { get; }

        private XFont MarginFont { get; }

        private double PointsPerInch { get; }

        private double Margin { get; }

        private double UseableX { get; }

        private double UseableY { get; }

        private int Rows { get; }

        private int Columns { get; }

        public int CardsPerPage { get; }

        private int ImagesDrawn { get; set; }

        private bool IsDrawing { get; set; }

        public async Task DrawImages(List<XImage> images)
        {
            while (IsDrawing)
            {
                await Task.Delay(1);
            }

            IsDrawing = true;

            if (images.Count + ImagesDrawn > CardsPerPage)
            {
                throw new InvalidOperationException($"Attempted to draw {images.Count + ImagesDrawn} to a page with a maximum of {Rows * Columns} images possible.");
            }

            foreach (XImage image in images)
            {
                // Do stuff
                int column = ImagesDrawn % Columns;
                int row = ImagesDrawn / Rows;

                XRect placement = await Task.Run(() => GetCardPlacement(row, column));
                await Task.Run(() => Gfx.DrawImage(image, placement));
                ImagesDrawn += 1;
            }

            IsDrawing = false;
        }

        private XRect GetCardPlacement(int row, int column)
        {
            if (row > Rows)
            {
                throw new InvalidOperationException($"Attempted to place an in in {row} when the max number of rows was {Rows}");
            }

            if (column > Columns)
            {
                throw new InvalidOperationException($"Attempted to place an in in {column} when the max number of columns was {Columns}");
            }

            // Calculate the position of the top left corner of the image.
            double xPos = Margin + column * (CardSize.Width + GutterThickness);
            double yPos = Margin + row * (CardSize.Height + GutterThickness);
            var position = new XPoint(xPos, yPos);
            var imagePlacement = new XRect(position, CardSize);
            return imagePlacement;
        }
    }
}
