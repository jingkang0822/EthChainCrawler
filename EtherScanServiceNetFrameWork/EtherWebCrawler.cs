using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class EtherWebCrawler
    {
        public IList<EtherWalletInfo> GetTopWallets()
        {
            var topWallets = new List<EtherWalletInfo>();

            for (int i = 1; i <= 4; i++)
            {
                topWallets.AddRange(GetTopWalletsByPage(i));
                Thread.Sleep((int)TimeSpan.FromSeconds(ConfigSettings.CrawlerWaitingInSeconds).TotalMilliseconds);
            }

            return topWallets;
        }

        public string GetNameTag(string address)
        {
            var httpRequester = new HttpRequester();
            var responsedHtml = httpRequester.GetHtmlResponse($"https://etherscan.io/address/{address}");

            if (responsedHtml == null) return string.Empty;

            var nameTag = responsedHtml.Split(new string[] { "Public Name Tag (viewable by anyone)'>" }, StringSplitOptions.None)
                .LastOrDefault().Split(new string[] { "</span>" }, StringSplitOptions.None)
                .FirstOrDefault();

            if (nameTag.Contains("!doctype html"))
            {
                nameTag = "NoFound";
            }
            else if (string.IsNullOrEmpty(nameTag))
            {
                nameTag = "ParseError";
            }
            nameTag = nameTag.Replace(",", " ");

            Console.WriteLine($"[GetWalletNameTag] success, nameTag: {nameTag}, address: {address}");
            return nameTag;
        }

        private IList<EtherWalletInfo> GetTopWalletsByPage(int page)
        {
            var httpRequester = new HttpRequester();
            var responsedHtml = httpRequester.GetHtmlResponse($"https://etherscan.io/accounts/{page}");

            var doc = new HtmlDocument();
            doc.LoadHtml(responsedHtml ?? string.Empty);
            var tableNode = doc.DocumentNode.SelectNodes("//table");
            var topWallets = new List<EtherWalletInfo>();

            if (tableNode != null)
            {
                foreach (var table in tableNode)
                {
                    foreach (var body in table.SelectNodes("tbody"))
                    {
                        foreach (var row in body.SelectNodes("tr"))
                        {
                            var cellId = 0;
                            var etherWalletInfo = new EtherWalletInfo();

                            foreach (var cell in row.SelectNodes("td"))
                            {
                                etherWalletInfo.SetTopAddressValue(cellId++, cell.InnerText);
                                Console.WriteLine(cell.InnerText);
                            }

                            topWallets.Add(etherWalletInfo);
                        }
                    }
                }
            }

            return topWallets;
        }
    }
}
