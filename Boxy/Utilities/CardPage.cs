using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace Boxy.Utilities
{
    public enum CutLineSizes
    {
        [Description("Small")]
        Small,

        [Description("Medium")]
        Medium,

        [Description("Quite Large")]
        QuiteLarge,

        [Description("Line-Colossus")]
        Colossal,

        [Description("A Line to surpass Metal Gear")]
        ALineToSurpassMetalGear,

        [Description("Chuck")]
        Chuck,
    }

    public class CardPage 
    {
        public CardPage(PdfPage page, double scalingPercent, bool hasCutLines, CutLineSizes cutLineSize, XKnownColor cutLineColor)
        {
            ScalingPercent = scalingPercent;
            HasCutLines = hasCutLines;
            CutLineSize = cutLineSize;
            CutLineColor = cutLineColor;
            Gfx = XGraphics.FromPdfPage(page);

            // Set Gutter
            GutterThickness = HasCutLines ? CutLineSize.ToPointSize() : 0;

            // Create a font.
            MarginFont = new XFont(FontFamily.GenericMonospace, 20, XFontStyle.Regular);

            // Set some properties other methods will need to use.
            PointsPerInch = page.Width.Point / page.Width.Inch;
            Margin = 0.25 * PointsPerInch;
            UseableX = page.Width - 2 * Margin;
            UseableY = page.Height - 2 * Margin;

            // MTG cards are 3.48 x 2.49 inches or 63 x 88 mm, then slightly scaled down to fit better in card sleeves.
            CardSize = new XSize(2.49 * PointsPerInch * ScalingPercent / 100 * 0.99, 3.48 * PointsPerInch * ScalingPercent / 100 * 0.99);

            // Predict the number of cards per row and cards per column.
            Rows = (int)((UseableY - GutterThickness) / (CardSize.Height + GutterThickness));
            Columns = (int)((UseableX - GutterThickness) / (CardSize.Width + GutterThickness));
            CardsPerPage = Rows * Columns;

            // Draw watermark
            Gfx.DrawString("Proxies by Boxy", MarginFont, XBrushes.AntiqueWhite, new XRect(0, -4, page.Width, page.Height), XStringFormats.TopCenter);
            Gfx.DrawString("Proxies by Boxy", MarginFont, XBrushes.AntiqueWhite, new XRect(0, 0, page.Width, page.Height + 2), XStringFormats.BottomCenter);
        }

        private double ScalingPercent { get; }

        private bool HasCutLines { get; }

        private CutLineSizes CutLineSize { get; }

        private XKnownColor CutLineColor { get; }

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

            var gutterPen = new XPen(XColor.FromKnownColor(CutLineColor), GutterThickness);

            foreach (XImage image in images)
            {
                // Do stuff
                int column = ImagesDrawn % Columns;
                int row = ImagesDrawn / Columns;

                XRect imagePlacement = await Task.Run(() => GetCardPlacement(row, column));

                if (HasCutLines)
                {
                    var verticalLinePlacement = new XRect(
                        new XPoint(imagePlacement.Left - GutterThickness / 2, imagePlacement.Top - GutterThickness), 
                        new XPoint(imagePlacement.Right + GutterThickness / 2, imagePlacement.Bottom + GutterThickness));

                    var horizontalLinePlacement = new XRect(
                        new XPoint(imagePlacement.Left - GutterThickness, imagePlacement.Top - GutterThickness / 2), 
                        new XPoint(imagePlacement.Right + GutterThickness, imagePlacement.Bottom + GutterThickness / 2));

                    await Task.Run(() => Gfx.DrawLine(gutterPen, horizontalLinePlacement.TopLeft, horizontalLinePlacement.TopRight));
                    await Task.Run(() => Gfx.DrawLine(gutterPen, verticalLinePlacement.TopRight, verticalLinePlacement.BottomRight));
                    await Task.Run(() => Gfx.DrawLine(gutterPen, horizontalLinePlacement.BottomRight, horizontalLinePlacement.BottomLeft));
                    await Task.Run(() => Gfx.DrawLine(gutterPen, verticalLinePlacement.BottomLeft, verticalLinePlacement.TopLeft));
                }

                await Task.Run(() => Gfx.DrawImage(image, imagePlacement));

                ImagesDrawn += 1;
            }

            IsDrawing = false;
        }

        private XRect GetCardPlacement(int row, int column)
        {
            if (row > Rows)
            {
                throw new InvalidOperationException($"Attempted to place an image in row {row} when the max number of rows was {Rows}");
            }

            if (column > Columns)
            {
                throw new InvalidOperationException($"Attempted to place an image in column {column} when the max number of columns was {Columns}");
            }

            // Calculate the position of the top left corner of the image.
            double xPos = Margin + GutterThickness + column * (CardSize.Width + GutterThickness);
            double yPos = Margin + GutterThickness + row * (CardSize.Height + GutterThickness);
            var position = new XPoint(xPos, yPos);
            var imagePlacement = new XRect(position, CardSize);
            return imagePlacement;
        }
    }
}
