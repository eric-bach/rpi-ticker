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
    /// <summary>
    /// Calls AWS Lambda Function URL with quotes from Yahoo Finance
    /// </summary>
    public static class YahooFinance
    {
        public static async void GetQuotesAsync(ConcurrentDictionary<string, QuoteViewModel> quotes)
        {
            //Console.WriteLine("INFO  Loading symbols");
            //var symbols = File.ReadAllLines("symbols.txt").ToArray();

            Console.WriteLine("INFO  Getting quotes");
            
            while (true)
            {
                try
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("apikey", "KFX7mmm%LuWYtKnDRiXr");
                    var response = await client.GetAsync($"https://mzxwah4s37265fs2i2j2ovhehe0tywwa.lambda-url.us-east-1.on.aws");
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<YahooFinanceQuote[]>(responseBody);

                    Console.WriteLine($"DEBUG Received {result.Length} quotes.");

                    foreach (var q in result)
                    {
                        Console.WriteLine($"{q.symbol} {q.currentPrice} {q.change}");
                        quotes[q.symbol] = new QuoteViewModel
                        {
                            Price = q.currentPrice,
                            Change = q.change
                        };
                    }

                    Console.WriteLine("DEBUG Waiting for next batch of quotes");
                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    Console.Write($"ERROR {e.Message}");
                }
            }
        }
    }
}
