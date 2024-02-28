// ReSharper disable InconsistentNaming
namespace EricBach.RpiTicker.ViewModels
{
    public class YahooFinanceQuote
    {
        public string symbol { get; set; }
        public decimal currentPrice { get; set; }
        public decimal change { get; set; }
        public decimal changePercent { get; set; }
        public string lastUpdated { get; set; }
        public long timestamp { get; set; }
    }
}