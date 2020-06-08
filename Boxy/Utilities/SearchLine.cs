using System;
using System.Collections.Generic;
using System.Linq;

namespace CardMimic.Utilities
{
    /// <summary>
    /// A parsed line from the user entry text box.
    /// </summary>
    public class SearchLine
    {
        /// <summary>
        /// Creates a new <see cref="SearchLine"/>, and populates all the relevant properties.
        /// </summary>
        public SearchLine(string searchTerm, int quantity)
        {
            SearchTerm = searchTerm;
            Quantity = quantity;
        }

        /// <summary>
        /// Creates a new <see cref="SearchLine"/>, and populates all the relevant properties.
        /// </summary>
        public SearchLine(string line)
        {
            ParseLine(line);
        }

        /// <summary>
        /// The search term to be used for finding the specific card.
        /// </summary>
        public string SearchTerm { get; private set; }

        /// <summary>
        /// User entered quantity, expected to be before the "card name."
        /// </summary>
        public int Quantity { get; private set; }

        private void ParseLine(string line)
        {
            List<string> splitLine = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
            string firstBlock = splitLine.First();
            splitLine.Remove(firstBlock);
            string otherBlocks = string.Join(" ", splitLine);
            var hasQuantity = false;

            // Get first non-whitespace block of characters, removing only whitespace stuff in the process.
            if (firstBlock.Any(char.IsDigit))
            {
                string quantityAsString = firstBlock.Replace("x", string.Empty).Replace("X", string.Empty);

                if (int.TryParse(quantityAsString, out int quantityAsInt))
                {
                    hasQuantity = true;
                    Quantity = quantityAsInt;
                }
                else
                {
                    Quantity = 1;
                }
            }
            else
            {
                Quantity = 1;
            }

            SearchTerm = hasQuantity ? otherBlocks : line.Trim();
        }

        public override string ToString()
        {
            return $"{Quantity} {SearchTerm}";
        }
    }
}
