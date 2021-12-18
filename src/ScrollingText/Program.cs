using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
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
                Brightness = 75,
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
                var quotes = GetQuotes();

                var s_len = 0;
                var n_len = 0;
                foreach (var q in quotes)
                {
                    s_len += _canvas.DrawText(font, pos + s_len, 13, new Color(255, 255, 0), $" {q.Symbol} {q.Price}");
                    s_len += _canvas.DrawText(font, pos + s_len, 13,
                        q.Change > 0 ? new Color(0, 255, 0) : new Color(255, 0, 0),
                        $"({(q.Change > 0 ? "+" : "")}{q.Change.ToString(CultureInfo.InvariantCulture)})");
                }

                n_len += _canvas.DrawText(font, pos - 10, 29, new Color(255, 255, 0),
                    "Diplomatic boycott of Beijing Olympics a 'symbolic' move that critics say doesn't go far enough"
                        .ToUpper());

                pos--;
                if (pos + Math.Max(s_len, n_len) < 0)
                {
                    pos = _canvas.Width;
                }

                Thread.Sleep(30);
                _canvas = matrix.SwapOnVsync(_canvas);
            }
        }


        private static IEnumerable<QuoteData> GetQuotes()
        {
            var path = "../../data/quotes.txt";
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

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _canvas.Clear();
        }
    }
}
