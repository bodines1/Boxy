using System;
using System.Linq;

namespace Boxy.Utilities
{
    public class SearchLine
    {
        public SearchLine(string line)
        {
            OriginalLine = line;
            ParseLine(line);
        }

        public string OriginalLine { get; }

        public string SearchTerm { get; private set; }

        public int Quantity { get; private set; }

        private void ParseLine(string line)
        {
            string firstChars = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).First();
            var foundQuantity = false;

            // Get first non-whitespace block of characters, removing only whitespace stuff in the process.
            if (firstChars.Any(char.IsDigit))
            {
                var quantityAsString = string.Empty;
                foreach (char c in firstChars.Where(char.IsDigit))
                {
                    quantityAsString.Append(c);
                }

                if (int.TryParse(quantityAsString, out int quantityAsInt))
                {
                    foundQuantity = true;
                    Quantity = quantityAsInt;
                }
                else
                {
                    foundQuantity = false;
                    Quantity = 1;
                }
            }
        }

        private string GetSeachTerm(string line)
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return $"{Quantity} {SearchTerm}";
        }
    }
}
