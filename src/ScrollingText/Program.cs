using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using rpi_rgb_led_matrix_sharp;

namespace ScrollingText
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

            Console.WriteLine("Loading configuration");

            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            Console.WriteLine("Initializing rpi-ticker");

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

            Console.WriteLine("Starting rpi-ticker");

            Parallel.Invoke(
                () => { GetQuotes(); },
                () => { GetHeadlines(); },
                () => { RunTicker(matrix); }
            );
        }


        private static async void GetQuotes()
        {
            var symbols = new[]
            {
                "AC.TO",
                "AMZN",
                "BMO.TO",
                "DDOG",
                "LYFT",
                "MSFT",
                "NIO",
                "PTON",
                "SNOW",
                "TD.TO",
                "TSLA",
                "VGRO.TO",
                "VXC.TO",
                "ZAG.TO",
                "ZRE.TO",
            };

            Console.WriteLine("Getting quotes");

            var i = 0;
            while (true)
            {
                var client = new HttpClient();
                var response =
                    await client.GetAsync(
                        $"https://query1.finance.yahoo.com/v10/finance/quoteSummary/{symbols[i++ % symbols.Length]}?modules=price");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Quote>(responseBody);
                Console.WriteLine(
                    $"{result.quoteSummary.result.First().price.symbol} {result.quoteSummary.result.First().price.regularMarketPrice.raw} {result.quoteSummary.result.First().price.regularMarketChange.raw}");

                _quotes[result.quoteSummary.result.First().price.symbol] = new QuoteSummary
                {
                    Price = result.quoteSummary.result.First().price.regularMarketPrice.raw,
                    Change = result.quoteSummary.result.First().price.regularMarketChange.raw
                };

                if (i != 0 && i % symbols.Length == 0)
                {
                    Console.WriteLine("Waiting for next batch of quotes");
                    Thread.Sleep(30000);
                }
            }
        }

        private static async void GetHeadlines()
        {
            Console.WriteLine("Getting headlines");

            var i = 0;
            while (true)
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
                    Console.WriteLine(title);
                    _headlines.Enqueue(title);
                }

                Console.WriteLine("Waiting for next news update");
                Thread.Sleep(60000);
            }
        }

        private static void RunTicker(RGBLedMatrix matrix)
        {
            var font = new RGBLedFont("../fonts/9x15B.bdf");
            var pos = _canvas.Width;

            Console.WriteLine("Scrolling text");

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
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13, new Color(0, 0, 255),
                            $" {q.Key} ");
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13, new Color(255, 255, 0),
                            $"{q.Value.Price:0.00} ");
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13,
                            q.Value.Change > 0 ? new Color(0, 255, 0) : new Color(255, 0, 0),
                            $"({(q.Value.Change > 0 ? "+" : "")}{q.Value.Change:0.00})");
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in _headlines)
                    {
                        headlinesLength += _canvas.DrawText(font, pos + headlinesLength, 29, new Color(255, 255, 0), $"{h.ToUpper()}");
                        headlinesLength += _canvas.DrawText(font, pos + headlinesLength, 29, new Color(255, 0, 0), " *** ");
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

                // Scroll text
                pos--;
                if (pos + Math.Max(quotesLength, headlinesLength) < 0)
                {
                    Console.WriteLine("WRAPPING TEXT");
                    pos = _canvas.Width;
                }

                Thread.Sleep(30);
                _canvas = matrix.SwapOnVsync(_canvas);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Interrupt: rpi-ticker shutting down");
            _canvas.Clear();
            Console.WriteLine("Good-bye");
        }
    }
}
