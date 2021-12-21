using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using rpi_rgb_led_matrix_sharp;

namespace EricBach.RpiTicker
{
    public class QuoteSummary
    {
        public decimal Price { get; set; }
        public decimal Change { get; set; }
    }

    public class Program
    {
        private static RGBLedCanvas _canvas;
        public static ConcurrentDictionary<string, QuoteSummary> _quotes { get; set; } = new ConcurrentDictionary<string, QuoteSummary>();
        public static ConcurrentQueue<string> _headlines { get; set; } = new ConcurrentQueue<string>();

        public static void Main()
        {
            // Clean up matrix on process exit
            Console.CancelKeyPress += OnProcessExit;

            Console.WriteLine("INFO  Loading configuration");

            // Load environment variables
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            Console.WriteLine("INFO  Loading symbols");

            var symbols = File.ReadAllLines("symbols.txt").ToArray();

            Console.WriteLine("INFO  Initializing rpi-ticker");

            var matrix = new RGBLedMatrix(new RGBLedMatrixOptions
            {
                Rows = 32,
                Cols = 64,
                ChainLength = 2,
                Brightness = 65,
                // ReSharper disable once StringLiteralTypo
                HardwareMapping = "adafruit-hat"
            });
            _canvas = matrix.CreateOffscreenCanvas();

            Console.WriteLine("INFO  Starting rpi-ticker");

            var quoteTask = Task.Run(() => { GetQuotes(symbols); });
            var headlinesTask = Task.Run(() => { GetHeadlines(); });
            Task.WaitAll(quoteTask, headlinesTask);

            RunTicker(matrix);
        }


        private static async void GetQuotes(string[] symbols)
        {
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

                    var result = JsonConvert.DeserializeObject<Quote>(responseBody);

                    Console.WriteLine(
                        $"DEBUG {result.quoteSummary.result.First().price.symbol} {result.quoteSummary.result.First().price.regularMarketPrice.raw} ({result.quoteSummary.result.First().price.regularMarketChange.raw})");

                    _quotes[result.quoteSummary.result.First().price.symbol] = new QuoteSummary
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
                catch (HttpRequestException e)
                {
                    // TODO Handle exception
                }
            }
        }

        private static async void GetHeadlines()
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

                    var result = JsonConvert.DeserializeObject<Headlines>(responseBody);

                    foreach (var title in result.results.Select(r => r.title))
                    {
                        Console.WriteLine($"DEBUG {title}");

                        if (!_headlines.Contains(title))
                        {
                            _headlines.Enqueue(title);
                        }
                    }

                    Console.WriteLine($"DEBUG {_headlines.Count} headlines in queue");
                    Thread.Sleep(3600000);
                }
                catch (HttpRequestException e)
                {
                }
            }
        }

        private static void RunTicker(RGBLedMatrix matrix)
        {
            var font = new RGBLedFont("../fonts/9x15B.bdf");
            var q_pos = _canvas.Width;
            var h_pos = _canvas.Width;

            Console.WriteLine("INFO  Starting scrolling ticker");

            while (true)
            {
                _canvas.Clear();

                // Print quotes and headlines to canvas
                var quotesLength = 0;
                var headlinesLength = 0;
                var quoteTask = Task.Run(() =>
                {
                    foreach (var q in _quotes)
                    {
                        quotesLength += _canvas.DrawText(font, q_pos + quotesLength, 13, new Color(0, 0, 255),
                            $" {q.Key} ");
                        quotesLength += _canvas.DrawText(font, q_pos + quotesLength, 13, new Color(255, 255, 0),
                            $"{q.Value.Price:0.00} ");
                        quotesLength += _canvas.DrawText(font, q_pos + quotesLength, 13,
                            q.Value.Change > 0 ? new Color(0, 255, 0) : new Color(255, 0, 0),
                            $"({(q.Value.Change > 0 ? "+" : "")}{q.Value.Change:0.00})");
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in _headlines)
                    {
                        headlinesLength += _canvas.DrawText(font, h_pos + headlinesLength, 29, new Color(255, 255, 0), $"{h.ToUpper()}");
                        headlinesLength += _canvas.DrawText(font, h_pos + headlinesLength, 29, new Color(255, 0, 0), " *** ");
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

                // Scroll text
                q_pos--;
                h_pos--;
                if (q_pos + quotesLength < 0)
                {
                    Console.WriteLine("DEBUG Re-scrolling quotes");
                    q_pos = _canvas.Width;
                }
                if (h_pos + headlinesLength < 0)
                {
                    Console.WriteLine("DEBUG Re-scrolling headlines");
                    h_pos = _canvas.Width;
                }

                Thread.Sleep(30);
                _canvas = matrix.SwapOnVsync(_canvas);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("INFO  Interrupted: rpi-ticker shutting down");
            _canvas.Clear();
            Console.WriteLine("INFO  Good-bye");
        }
    }
}