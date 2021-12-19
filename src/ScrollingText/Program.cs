using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using rpi_rgb_led_matrix_sharp;

namespace ScrollingText
{
    public class QuoteData
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
    }

    public class Quote
    {
        public QuoteSummary quoteSummary { get; set; }

        public class QuoteSummary
        {
            public ICollection<Result> result { get; set; }

            public class Result
            {
                public Price price { get; set; }

                public class Price
                {
                    public string symbol { get; set; }
                    public RegularMarketPrice regularMarketPrice { get; set; }
                    public RegularMarketChange regularMarketChange { get; set; }

                    public class RegularMarketPrice
                    {
                        public decimal raw { get; set; }
                    }

                    public class RegularMarketChange
                    {
                        public decimal raw { get; set; }
                    }
                }
            }
        }
    }

    public class Program
    {
        private static RGBLedCanvas _canvas;

        public static void Main()
        {
            // Clean up matrix on process exit
            Console.CancelKeyPress += OnProcessExit;

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
                () => { GetData(); }, 
                () => { RunTicker(matrix); }
            );
        }


        private static async void GetData()
        {
            Console.WriteLine("Getting quotes");

            var symbols = new string[]
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
                Console.Write(result.quoteSummary.result.First().price.symbol);
                Console.Write(result.quoteSummary.result.First().price.regularMarketPrice.raw);
                Console.WriteLine(result.quoteSummary.result.First().price.regularMarketChange.raw);

                Thread.Sleep(5000);
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

                // Read quotes and headlines from file
                IEnumerable<QuoteData> quotes = new List<QuoteData>();
                IEnumerable<string> headlines = new List<string>();
                var getQuotesTask = Task.Run(() => { quotes = GetQuotes(); });
                var getHeadlinesTask = Task.Run(() => { headlines = GetHeadlines(); });
                Task.WaitAll(getQuotesTask, getHeadlinesTask);

                // Print quotes and headlines to canvas
                var quotesLength = 0;
                var headlinesLength = 0;
                var quoteTask = Task.Run(() =>
                {
                    foreach (var q in quotes)
                    {
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13, new Color(0, 0, 255),
                            $" {q.Symbol} ");
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13, new Color(255, 255, 0),
                            $"{q.Price:0.00} ");
                        quotesLength += _canvas.DrawText(font, pos + quotesLength, 13,
                            q.Change > 0 ? new Color(0, 255, 0) : new Color(255, 0, 0),
                            $"({(q.Change > 0 ? "+" : "")}{q.Change:0.00})");
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in headlines)
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
                    pos = _canvas.Width;
                }

                Thread.Sleep(30);
                _canvas = matrix.SwapOnVsync(_canvas);
            }
        }

        private static IEnumerable<QuoteData> GetQuotes()
        {
            const string path = "../data/quotes.txt";
            var lines = File.ReadAllLines(path);

            var obj = new List<QuoteData>();
            foreach (var line in lines)
            {
                var values = line.Split(',');
                obj.Add(new QuoteData
                {
                    Symbol = values[0],
                    Price = decimal.Parse(values[1]),
                    Change = decimal.Parse(values[2]),
                });
            }

            return obj;
        }

        private static IEnumerable<string> GetHeadlines()
        {
            const string path = "../data/headlines.txt";
            return File.ReadAllLines(path).ToList().Take(4);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Interrupt: rpi-ticker shutting down");
            _canvas.Clear();
            Console.WriteLine("Good-bye");
        }
    }
}
