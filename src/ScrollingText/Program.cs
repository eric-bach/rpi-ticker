using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static void Main(string[] args)
        {
            Console.WriteLine("Started");

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

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

            RunTicker(matrix);
        }

        private static void RunTicker(RGBLedMatrix matrix)
        {
            // TODO Find a bigger font
            var font = new RGBLedFont("../fonts/9x15B.bdf");
            var pos = _canvas.Width;

            while (true)
            {
                _canvas.Clear();

                // TODO Get stock prices and news from file in a thread
                IEnumerable<QuoteData> quotes = new List<QuoteData>();
                IEnumerable<string> headlines = new List<string>();
                var getQuotesTask = Task.Run(() =>
                {
                    quotes = GetQuotes();
                });
                var getHeadlinesTask = Task.Run(() =>
                {
                    headlines = GetHeadlines();
                });
                Task.WaitAll(getQuotesTask, getHeadlinesTask);

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
                    //headlinesLength += _canvas.DrawText(font, pos - 10, 29, new Color(255, 255, 0), headlines.First().ToUpper());
                    foreach (var h in headlines)
                    {
                        headlinesLength += _canvas.DrawText(font, pos + headlinesLength, 29, new Color(255, 255, 0), $"{h.ToUpper()}  ");
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

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
            return File.ReadAllLines(path).ToList().Take(2);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _canvas.Clear();
        }
    }
}
