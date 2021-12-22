using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace EricBach.RpiTicker.ViewModels
{
    // ReSharper disable once IdentifierTypo
    public class NewsdataHeadlines
    {
        public ICollection<NewsResult> results { get; set; } = new List<NewsResult>();

        public class NewsResult
        {
            public string title { get; set; }
        }
    }
}
