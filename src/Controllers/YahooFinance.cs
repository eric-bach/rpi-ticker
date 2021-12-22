using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using EricBach.RpiTicker.ViewModels;
using Newtonsoft.Json;

namespace EricBach.RpiTicker.Controllers
{
    public static class YahooFinance
    {
        public static async void GetQuotesAsync(ConcurrentDictionary<string, QuoteViewModel> quotes)
        {
            Console.WriteLine("INFO  Loading symbols");
            var symbols = (await File.ReadAllLinesAsync("symbols.txt")).ToArray();

            Console.WriteLine("INFO  Getting quotes");
            
            var i = 0;
            while (true)
            {
                try
                {
                    var client = new HttpClient();
                    var response =
                        await client.GetAsync(
                            $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{symbols[i++ % symbols.Length]}?modules=price");
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<YahooFinanceQuote>(responseBody);

                    Console.WriteLine(
                        $"DEBUG {result.quoteSummary.result.First().price.symbol} {result.quoteSummary.result.First().price.regularMarketPrice.raw} ({result.quoteSummary.result.First().price.regularMarketChange.raw})");

                    quotes[result.quoteSummary.result.First().price.symbol] = new QuoteViewModel
                    {
                        Price = result.quoteSummary.result.First().price.regularMarketPrice.raw,
                        Change = result.quoteSummary.result.First().price.regularMarketChange.raw
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
        }
    }
}
