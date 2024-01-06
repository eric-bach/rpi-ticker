using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using EricBach.RpiTicker.Controllers;
using EricBach.RpiTicker.Utils;
using EricBach.RpiTicker.ViewModels;

namespace EricBach.RpiTicker
{
    public class Program
    {
        private static readonly MatrixController Matrix = new MatrixController();
        private static ConcurrentDictionary<string, QuoteViewModel> _quotes { get; } = new ConcurrentDictionary<string, QuoteViewModel>();
        private static ConcurrentQueue<string> _headlines { get; } = new ConcurrentQueue<string>();

        public static void Main()
        {
            // Clean up Matrix on process exit
            Console.CancelKeyPress += OnProcessExit;

            Console.WriteLine("INFO  Loading configuration");

            // Load environment variables
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            Console.WriteLine("INFO  Starting rpi-ticker");

            //var quotesTask = Task.Run(() => { YahooFinanceApi.GetQuotesAsync(_quotes); });
            var quotesTask = Task.Run(() => { YahooFinanceHtml.GetQuotes(_quotes); });
            var headlinesTask = Task.Run(() => { NewsData.GetHeadlines(_headlines); });
            Task.WaitAll(quotesTask, headlinesTask);

            Matrix.RunTicker(_quotes, _headlines);
        }
        
        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("INFO  Interrupted: rpi-ticker shutting down");
            Matrix.Clear();
            Console.WriteLine("INFO  Good-bye");
        }
    }
}