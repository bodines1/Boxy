using PdfSharp.Drawing;

namespace CardMimic.Utilities
{
    public class CardPdfImage
    {
        public CardPdfImage(XImage xImage, int row, int column, bool isFront)
        {
            XImage = xImage;
            Row = row;
            Column = column;
            IsFront = isFront;
        }

        public XImage XImage { get; }

        public int Row { get; }

        public int Column { get; }

        public bool IsFront { get; }
    }
}
