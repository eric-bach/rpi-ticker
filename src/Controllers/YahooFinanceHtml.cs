using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricBach.RpiTicker.ViewModels;
using HtmlAgilityPack;

namespace EricBach.RpiTicker.Controllers
{
    public static class YahooFinanceHtml
    {
        public static async Task GetQuotesAsync(ConcurrentDictionary<string, QuoteViewModel> quotes)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("INFO  Loading symbols");
                var symbols = File.ReadAllLines("symbols.txt").ToArray();
                Console.WriteLine("INFO  Getting quotes");

                var i = 0;
                while (true)
                {
                    try
                    {
                        var symbol = symbols[i++ % symbols.Length];

                        var html = @"https://finance.yahoo.com/quote/AAPL";
                        var web = new HtmlWeb();
                        var htmlDoc = web.Load(html);

                        var node = htmlDoc.DocumentNode.SelectSingleNode($"//*[@data-symbol='{symbol}']");
                        var node2 = htmlDoc.DocumentNode.SelectSingleNode("//*[@data-field='regularMarketChange']");
                        var node3 = htmlDoc.DocumentNode.SelectSingleNode("//*[@data-field='regularMarketChangePercent']");

                        var quote = decimal.Round(decimal.Parse(node.Attributes["value"].Value), 2);
                        var change = decimal.Round(decimal.Parse(node2.Attributes["value"].Value), 2);
                        var changePercent = decimal.Round(decimal.Parse(node3.Attributes["value"].Value), 2);

                        Console.WriteLine($"DEBUG {symbol} {quote} ({change})");

                        quotes[symbol] = new QuoteViewModel
                        {
                            Price = quote,
                            Change = change
                        };

                        if (i != 0 && i % symbols.Length == 0)
                        {
                            Console.WriteLine("DEBUG Waiting for next batch of quotes");
                            Thread.Sleep(60000);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write($"ERROR {e.Message}");
                    }
                }
            });
        }
    }
}
