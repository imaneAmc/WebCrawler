using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.CrawlerModels
{
   public class CrawlExtension
    {
        public string SearchWord { get; set; }
        public int  NumberSearchFound { get; set; }
        public List<String> ListUrls { get; set; }


    }
}
