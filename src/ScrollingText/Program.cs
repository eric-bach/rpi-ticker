using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using rpi_rgb_led_matrix_sharp;

namespace ScrollingText
{
    public class QuoteData
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
    }

    public class Program
    {
        private static RGBLedCanvas _canvas;

        public static void Main()
        {
            // Clean up matrix on process exit
            Console.CancelKeyPress += OnProcessExit;
            
            /*
            while (true)
            {
                // Read quotes and headlines from file
                IEnumerable<QuoteData> quotes = new List<QuoteData>();
                IEnumerable<string> headlines = new List<string>();
                var getQuotesTask = Task.Run(() => { quotes = GetQuotes(); });
                var getHeadlinesTask = Task.Run(() => { headlines = GetHeadlines(); });
                Task.WaitAll(getQuotesTask, getHeadlinesTask);

                var tempQuotes = string.Empty;
                var tempHeadlines = string.Empty;

                // Print quotes and headlines to canvas
                var quotesLength = 0;
                var headlinesLength = 0;
                var quoteTask = Task.Run(() =>
                {
                    foreach (var q in quotes)
                    {
                        var a = $" {q.Symbol} ";
                        var b = $"{q.Price:0.00} ";
                        var c = $"({(q.Change > 0 ? "+" : "")}{q.Change:0.00})";

                        tempQuotes += $"{a}{b}{c}";
                        quotesLength += a.Length + b.Length + c.Length;
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in headlines)
                    {
                        var d = "{h.ToUpper()}  ";

                        tempHeadlines += $"{d}";
                        headlinesLength += d.Length;
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

                if (Math.Abs(quotesLength - headlinesLength) <= 8) continue;
                if (quotesLength > headlinesLength)
                {
                    var mag = Math.Round((double)quotesLength / headlinesLength, MidpointRounding.AwayFromZero);
                    for (var i = 1; i < mag; i++)
                    {
                        tempHeadlines.Concat(tempHeadlines);
                    }
                }
                else
                {
                    var mag = Math.Round((double)headlinesLength / headlinesLength, MidpointRounding.AwayFromZero);
                    for (var i = 1; i < mag; i++)
                    {
                        tempQuotes.Contains(tempQuotes);
                    }
                }
            }
            */

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

            RunTicker(matrix);
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
                var quoteString = "";
                var headlineString = "";
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

                        quoteString = string.Join(quoteString, $" {q.Symbol} {q.Price:0.00} ({(q.Change > 0 ? "+" : "")}{q.Change:0.00})");
                        Console.WriteLine(quoteString);
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in headlines)
                    {
                        headlinesLength += _canvas.DrawText(font, pos + headlinesLength, 29, new Color(255, 255, 0), $"{h.ToUpper()}  ");

                        headlineString = string.Join(headlineString, $"{h.ToUpper()}");
                        Console.WriteLine(headlineString);
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

                // Normalize strings
                if (Math.Abs(quotesLength - headlinesLength) <= 8)
                {
                    Console.WriteLine("All good");
                    continue;
                }
                if (quotesLength > headlinesLength)
                {
                    var mag = Math.Round((double)quotesLength / headlinesLength, MidpointRounding.AwayFromZero);
                    for (var i = 1; i < mag; i++)
                    {
                        headlineString = string.Join(headlineString, headlineString);
                        Console.WriteLine("Joining Headline String");
                    }
                }
                else
                {
                    var mag = Math.Round((double)headlinesLength / headlinesLength, MidpointRounding.AwayFromZero);
                    for (var i = 1; i < mag; i++)
                    {
                        quoteString = string.Join(quoteString, quoteString);
                        Console.WriteLine("Joining Quote String");
                    }
                }

                //Console.WriteLine($"Quote String: {quoteString.Length} HeadlineString: {headlineString.Length}");

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
