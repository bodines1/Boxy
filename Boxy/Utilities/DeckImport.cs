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
    public static class DeckImport
    {
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

            throw new InvalidOperationException("The URL provided does not appear to point to a website Boxy is able to import from.\r\n\r\n" +
                                                "Currently supported\r\n" +
                                                "\t\u2765TappedOut.net\r\n" +
                                                "\t\u2765MtgGoldfish.net\r\n");
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
                reporter.Progress(web, i, 0, nodes.Count);
                reporter.Report($"Bifurcating the furcate {i}/{nodes.Count}");

                try
                {
                    HtmlNode node = nodes[i];
                    string name = node.Attributes.Single(a => a.Name == "data-name").Value;
                    int qty = int.Parse(node.Attributes.Single(a => a.Name == "data-qty").Value);
                    var line = new SearchLine(name, qty);
                    decklistBuilder.AppendLine(line.ToString());
                }
                catch (Exception)
                {
                    reporter.Report(web, $"Failed to import node #{i} from {url}", true);
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
                reporter.Progress(web, i, 0, deckNodes.Count);
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
                    reporter.Report(web, $"Failed to import node #{i} from {url}", true);
                }
            }

            reporter.StopProgress();
            return decklistBuilder.ToString();
        }
    }
}
