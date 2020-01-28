using Boxy.Reporting;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Boxy.Utilities
{
    /// <summary>
    /// Has methods to attempt to parse a url as either a web address or local file, and convert it to a Boxy readable decklist.
    /// </summary>
    public static class DeckImport
    {
        /// <summary>
        /// Attempts to parse a url as either a web address or local file, and convert it to a Boxy readable decklist.
        /// </summary>
        /// <param name="url">The URL to look for the raw deck list.</param>
        /// <param name="reporter">Object which can report status updates back to subscribers.</param>
        /// <returns>The parsed deck list.</returns>
        public static async Task<string> ImportFromUrl(string url, IReporter reporter)
        {
            if (url.ToLower().Contains("tappedout.net"))
            {
                return await ImportFromTappedOut(url, reporter);
            }

            if (url.ToLower().Contains("mtggoldfish.com"))
            {
                return await ImportFromMtgGoldfish(url, reporter);
            }

            if (url.ToLower().Contains("aetherhub.com"))
            {
                return await ImportFromAetherhub(url, reporter);
            }

            if (url.ToLower().Contains("streamdecker.com"))
            {
                return await ImportFromStreamdecker(url, reporter);
            }

            throw new InvalidOperationException("The URL provided does not appear to point to a website Boxy is able to import from.\r\n\r\n" +
                                                "--Currently supported--\r\n\r\n" +
                                                "\t\u2765 TappedOut.net\r\n" +
                                                "\t\u2765 MtgGoldfish.com\r\n" +
                                                "\t\u2765 Aetherhub.com\r\n");
        }

        private static async Task<string> ImportFromTappedOut(string url, IReporter reporter)
        {
            var web = new HtmlWeb();
            reporter.Report("Unraveling skeins...");
            HtmlDocument doc = await web.LoadFromWebAsync(url);
            var decklistBuilder = new StringBuilder();

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='boardlist']/li/a"); // Looking for data-name in span from these nodes

            if (nodes == null || !nodes.Any())
            {
                throw new InvalidOperationException("Could not find a valid deck at the URL. Make sure the link provided is pointing to the root of the deck.");
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                await Task.Delay(1);
                reporter.Progress(i, 0, nodes.Count);
                reporter.Report($"Bifurcating the furcate {i}/{nodes.Count}");

                try
                {
                    HtmlNode node = nodes[i];
                    string name = HttpUtility.HtmlDecode(node.Attributes.Single(a => a.Name == "data-name").Value.Trim());
                    int qty = int.Parse(node.Attributes.Single(a => a.Name == "data-qty").Value);
                    var line = new SearchLine(name, qty);
                    decklistBuilder.AppendLine(line.ToString());
                }
                catch (Exception)
                {
                    reporter.Report($"Failed to import node #{i} from {url}", true);
                }
            }

            return decklistBuilder.ToString();
        }

        private static async Task<string> ImportFromMtgGoldfish(string url, IReporter reporter)
        {
            var web = new HtmlWeb();
            reporter.Report("Unraveling skeins...");
            HtmlDocument doc = await web.LoadFromWebAsync(url);
            var decklistBuilder = new StringBuilder();

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//table[@class='deck-view-deck-table']/tr");
            List<HtmlNode> deckNodes = nodes.TakeWhile(node => !node.OuterHtml.Contains("Cards Total")).ToList();

            if (nodes == null || !nodes.Any())
            {
                throw new InvalidOperationException("Could not find a valid deck at the URL. Make sure the link provided is pointing to the root of the deck.");
            }

            reporter.StartProgress();

            for (var i = 0; i < deckNodes.Count; i++)
            {
                await Task.Delay(1);
                reporter.Progress(i, 0, deckNodes.Count);
                reporter.Report($"Bifurcating the furcate {i}/{deckNodes.Count}");

                HtmlNode node = deckNodes[i];
                try
                {
                    HtmlNodeCollection qtyNodes = node.SelectNodes(".//td[@class='deck-col-qty']");
                    HtmlNodeCollection nameNodes = node.SelectNodes(".//td[@class='deck-col-card']");

                    if (qtyNodes?.Count != 1 || nameNodes?.Count != 1)
                    {
                        continue;
                    }

                    int qty = int.Parse(qtyNodes[0].InnerText.Trim());
                    string name = HttpUtility.HtmlDecode(nameNodes[0].InnerText.Trim());
                    var line = new SearchLine(name, qty);
                    decklistBuilder.AppendLine(line.ToString());
                }
                catch (Exception)
                {
                    reporter.Report($"Failed to import node #{i} from {url}", true);
                }
            }

            reporter.StopProgress();
            return decklistBuilder.ToString();
        }

        private static async Task<string> ImportFromAetherhub(string url, IReporter reporter)
        {
            var web = new HtmlWeb();
            reporter.Report("Unraveling skeins...");
            HtmlDocument doc = await web.LoadFromWebAsync(url);
            var decklistBuilder = new StringBuilder();

            HtmlNodeCollection activeTabNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ',normalize-space(@class),' '),' active ')]");

            if (activeTabNodes == null || activeTabNodes.Count != 1)
            {
                throw new InvalidOperationException("Could not find a valid deck at the URL. Make sure the link provided is pointing to the root of the deck.");
            }

            HtmlNodeCollection cardNodes = activeTabNodes.Single().SelectNodes(".//div[@class='card-container']/div[@class='column-wrapper']/div/a[@class='cardLink']");

            reporter.StartProgress();
            var cardIndex = 0;
            var qty = 1;

            while (cardIndex < cardNodes.Count)
            {
                try
                {
                    string name = cardNodes[cardIndex].Attributes.Single(a => a.Name == "data-card-name").Value;
                    qty = 1;

                    while (cardIndex + qty < cardNodes.Count && cardNodes[cardIndex + qty].Attributes.Single(a => a.Name == "data-card-name").Value == name)
                    {
                        qty++;
                    }

                    cardIndex += qty;
                    name = HttpUtility.HtmlDecode(name.Trim());
                    var line = new SearchLine(name, qty);
                    decklistBuilder.AppendLine(line.ToString());
                }
                catch (Exception)
                {
                    reporter.Report($"Failed to import all or part of node #{cardIndex} from {url}", true);
                    cardIndex += qty;
                }
            }

            reporter.StopProgress();
            return decklistBuilder.ToString();
        }

        private static async Task<string> ImportFromStreamdecker(string url, IReporter reporter)
        {
            var web = new HtmlWeb();
            reporter.Report("Unraveling skeins...");
            HtmlDocument doc = await web.LoadFromWebAsync(url);

            var decklistBuilder = new StringBuilder();

            HtmlNodeCollection activeTabNodes = doc.DocumentNode.SelectNodes("//div[@class='display-card-list']");

            if (activeTabNodes == null || activeTabNodes.Count != 1)
            {
                throw new InvalidOperationException("Could not find a valid deck at the URL. Make sure the link provided is pointing to the root of the deck.");
            }

            HtmlNodeCollection cardNodes = activeTabNodes.Single().SelectNodes(".//div[@class='card-container']/div[@class='column-wrapper']/div/a[@class='cardLink']");

            reporter.StartProgress();
            var cardIndex = 0;
            var qty = 1;

            while (cardIndex < cardNodes.Count)
            {
                try
                {
                    string name = cardNodes[cardIndex].Attributes.Single(a => a.Name == "data-card-name").Value;
                    qty = 1;

                    while (cardIndex + qty < cardNodes.Count && cardNodes[cardIndex + qty].Attributes.Single(a => a.Name == "data-card-name").Value == name)
                    {
                        qty++;
                    }

                    cardIndex += qty;
                    name = HttpUtility.HtmlDecode(name.Trim());
                    var line = new SearchLine(name, qty);
                    decklistBuilder.AppendLine(line.ToString());
                }
                catch (Exception)
                {
                    reporter.Report($"Failed to import all or part of node #{cardIndex} from {url}", true);
                    cardIndex += qty;
                }
            }

            reporter.StopProgress();
            return decklistBuilder.ToString();
        }
    }
}
