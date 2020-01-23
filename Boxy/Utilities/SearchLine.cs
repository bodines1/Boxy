namespace Boxy.Utilities
{
    public class SearchLine
    {
        public SearchLine(string line)
        {

        }

        public string OriginalLine { get; }

        public string SearchTerm { get; }

        public int Quantity { get; }

        private string GetQuantity()
        {
            // Possible patterns?
            // "n"
            // "xn"
            // "x n" <- Can't handle?
            // "nx"
            // Assume first term must contain number of cards? What about last term? "Time Wipe x4" ?

            if (true)
            {

            }

            return string.Empty;
        }

        private string GetSeachTerm()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return $"{Quantity} {SearchTerm}";
        }
    }
}
