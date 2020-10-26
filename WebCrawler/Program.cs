using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebCrawler.CrawlerModels;

namespace WebCrawler
{
    class Program
    {
        public static void Main(string[] args)
        {
            var UrlsList = new List<string>();
            var SearchWord = "";

            try
            {
                //Initiate Validation
                ServiceModel.InitiateArgs(args, out UrlsList,out  SearchWord);

                //Use Abot Logger
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateLogger();

                Log.Logger.Information("Starting Test!");

                
                if(UrlsList.Count>0 && String.IsNullOrEmpty(SearchWord)==false)
                {
                    //Initiate simple crawler
                    SimpleCrawler(UrlsList, SearchWord).Wait();

                }
               

            }
            catch(Exception e)
            {
                // log any error 
                Console.WriteLine("\r\n an error occured while processing requests {0} ", e);

            }

           
            
            Console.ReadKey();
         
        }

     
        /// <summary>
        /// Initiate crawling with
        /// List of urls to be crawled 
        /// Search text to be found 
        /// configure crawling
        /// </summary>
        /// <param name="UrlsList"></param>
        /// <param name="SearchText"></param>
        /// <returns></returns>
        private static async Task SimpleCrawler(List<string> UrlsList, string SearchText)
        {
            var ListPagesUrs = new List<string>();
            int TotalNumer = 0;
           
            //loop through a list 
            foreach(var el in UrlsList)
            {
                var config = new CrawlConfiguration
                {
                    MaxPagesToCrawl = 10, //Only crawl 50 pages
                    MinCrawlDelayPerDomainMilliSeconds = 3000 //Wait this many millisecs between requests
                };

                // use polite crawler 
                var crawler = new PoliteWebCrawler(config);

                crawler.PageCrawlStarting += crawler_ProcessPageCrawlStarting;
                crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...
                crawler.PageCrawlDisallowed += crawler_PageCrawlDisallowed;
                crawler.PageLinksCrawlDisallowed+= crawler_PageLinksCrawlDisallowed;
               
                //Setup Custom class to store data
                crawler.CrawlBag.CrawlExtension = new CrawlExtension()
                {
                    SearchWord = SearchText,
                    ListUrls = new System.Collections.Generic.List<string>()

                };

                var test = new List<string>();

                var crawlResult = await crawler.CrawlAsync(new Uri(el));

                
                List<string> valueList = crawler.CrawlBag.CrawlExtension.ListUrls;

                // add  urls found to list
                ListPagesUrs.AddRange(valueList);
                TotalNumer = TotalNumer + crawler.CrawlBag.CrawlExtension.NumberSearchFound;//increment values found

            }


            Console.WriteLine("==== Total number of sites with word [{0}] is {1}", SearchText,TotalNumer);
            Console.WriteLine("==== Site lists with word {0} =====\r", SearchText);
            foreach (var el in ListPagesUrs)
            {
                Console.WriteLine(el);

            }


        }
        /// <summary>
        /// event triggers when crawling link is not possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static  void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        /// <summary>
        /// event for disallowed crawling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private static async Task DemoSinglePageRequest()
        {
            var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            var crawledPage = await pageRequester.MakeRequestAsync(new Uri("http://google.com"));
            Log.Logger.Information("{result}", new
            {
                url = crawledPage.Uri,
                status = Convert.ToInt32(crawledPage.HttpResponseMessage.StatusCode)
            });
        }

        /// <summary>
        /// event when crawling start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }
        /// <summary>
        /// event for when crawling is completed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var httpStatus = e.CrawledPage.HttpResponseMessage.StatusCode;
            var rawPageText = e.CrawledPage.Content.Text;
            var searchWord = e.CrawlContext.CrawlBag.CrawlExtension.SearchWord;
            var SearchRegex = "([\\s\\S] *)()("+searchWord+ ")([\\s\\S] *)";//Search Pattern
            
            Regex RegexElement = new Regex(SearchRegex, RegexOptions.IgnoreCase);
            //Setup Regex 
            Match isMatch=RegexElement.Match(rawPageText);

            //check if rawcontent match the pattern
            if(isMatch.Success)
            {
                Console.WriteLine("your Search word [{0}] has been found in this page : {1}", searchWord,e.CrawledPage.Uri.OriginalString);
                e.CrawlContext.CrawlBag.CrawlExtension.NumberSearchFound++;
                e.CrawlContext.CrawlBag.CrawlExtension.ListUrls.Add(e.CrawledPage.Uri.OriginalString);

            }


        }
      
    }
}
