using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading;
using EricBach.RpiTicker.ViewModels;
using Newtonsoft.Json;

namespace EricBach.RpiTicker.Controllers
{
    public static class NewsData
    {
        public static async void GetHeadlines(ConcurrentQueue<string> headlines)
        {
            Console.WriteLine("INFO  Getting headlines");

            while (true)
            {
                try
                {
                    var client = new HttpClient();
                    var response =
                        await client.GetAsync(
                            $"https://newsdata.io/api/1/news?apikey={Environment.GetEnvironmentVariable("NEWSDATA_API_KEY")}&language=en&country=ca&q=headlines");

                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<NewsdataHeadlines>(responseBody);

                    foreach (var title in result.results.Select(r => r.title))
                    {
                        Console.WriteLine($"DEBUG {title}");

                        if (!headlines.Contains(title))
                        {
                            headlines.Enqueue(title);
                        }
                    }

                    Console.WriteLine($"DEBUG {headlines.Count} headlines in queue");
                    Thread.Sleep(3600000);
                }
                catch (Exception e)
                {
                    Console.Write($"ERROR {e.Message}");
                }
            }
        }
    }
}
