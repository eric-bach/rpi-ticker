using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EricBach.RpiTicker.ViewModels;
using rpi_rgb_led_matrix_sharp;

namespace EricBach.RpiTicker.Controllers
{
    public class MatrixController
    {
        private RGBLedMatrix Matrix { get;  }
        private RGBLedCanvas Canvas { get; set; }

        // ReSharper disable once StringLiteralTypo
        public MatrixController(int rows = 32, int cols = 64, int chainLength = 2, int brightness = 50, string hardwareMapping = "adafruit-hat")
        {
            Console.WriteLine("INFO  Initializing rpi-ticker");

            Matrix = new RGBLedMatrix(new RGBLedMatrixOptions
            {
                Rows = rows,
                Cols = cols,
                ChainLength = chainLength,
                Brightness = brightness,
                HardwareMapping = hardwareMapping
            });
            Canvas = Matrix.CreateOffscreenCanvas();
        }

        public void RunTicker(ConcurrentDictionary<string, QuoteViewModel> quotes, ConcurrentQueue<string> headlines)
        {
            var font = new RGBLedFont("../fonts/9x15B.bdf");
            var q_pos = Canvas.Width;
            var h_pos = Canvas.Width;

            Console.WriteLine("INFO  Starting scrolling ticker");

            while (true)
            {
                Canvas.Clear();

                // Print quotes and headlines to canvas
                var quotesLength = 0;
                var headlinesLength = 0;
                var quoteTask = Task.Run(() =>
                {
                    foreach (var q in quotes)
                    {
                        quotesLength += Canvas.DrawText(font, q_pos + quotesLength, 13, new Color(0, 0, 255), $" {q.Key} ");
                        quotesLength += Canvas.DrawText(font, q_pos + quotesLength, 13, new Color(255, 255, 0), $"{q.Value.Price:0.00} ");
                        quotesLength += Canvas.DrawText(font, q_pos + quotesLength, 13, q.Value.Change > 0 ? new Color(0, 255, 0) : new Color(255, 0, 0),
                            $"({(q.Value.Change > 0 ? "+" : "")}{q.Value.Change:0.00})");
                    }
                });
                var headlineTask = Task.Run(() =>
                {
                    foreach (var h in headlines)
                    {
                        headlinesLength += Canvas.DrawText(font, h_pos + headlinesLength, 29, new Color(255, 255, 0), $"{h.ToUpper()}");
                        headlinesLength += Canvas.DrawText(font, h_pos + headlinesLength, 29, new Color(255, 0, 0), " *** ");
                    }
                });
                Task.WaitAll(quoteTask, headlineTask);

                // Scroll text
                q_pos--;
                h_pos--;
                if (q_pos + quotesLength < 0)
                {
                    Console.WriteLine("DEBUG Re-scrolling quotes");
                    q_pos = Canvas.Width;
                }
                if (h_pos + headlinesLength < 0)
                {
                    Console.WriteLine("DEBUG Re-scrolling headlines");
                    h_pos = Canvas.Width;
                }

                Thread.Sleep(30);
                Canvas = Matrix.SwapOnVsync(Canvas);
            }
        }

        public void Clear()
        {
            Canvas.Clear();
        }
    }
}
