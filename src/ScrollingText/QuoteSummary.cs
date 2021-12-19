using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace ScrollingText
{
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
}
