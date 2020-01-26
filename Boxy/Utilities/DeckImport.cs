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
    public enum SupportedImportWebsites
    {
        TappedOut,
        MtgGoldfish,
    }

    public static class DeckImport
    {
        public static async Task<string> ImportFromUrl(SupportedImportWebsites importFrom, string url, IReporter reporter)
        {
            switch (importFrom)
            {
                case SupportedImportWebsites.TappedOut:
                    return await ImportFromTappedOut(url, reporter);
                case SupportedImportWebsites.MtgGoldfish:
                    return await ImportFromMtgGoldfish(url, reporter);
                default:
                    throw new ArgumentOutOfRangeException(nameof(importFrom), importFrom, @"Enum value not handled in switch, support for this website has not been added.");
            }
        }

        private static async Task<string> ImportFromTappedOut(string url, IReporter reporter)
        {
            var web = new HtmlWeb();
            reporter.Report("Unraveling skeins...");
            HtmlDocument doc = await web.LoadFromWebAsync(url);
            var decklistBuilder = new StringBuilder();

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//ul[@class='boardlist']/li/a"); // Looking for data-name in span from these nodes

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

            if (!deckNodes.Any())
            {
                reporter.Report(web, $"Failed to import from {url}, the site does not appear to be a normal MtgGoldfish deck list.", true);
                return string.Empty;
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
