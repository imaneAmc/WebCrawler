using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.CrawlerModels
{
    public class ServiceModel
    {
        public static void InitiateArgs(string[] args, out List<string> UrlsList, out string SearchWord)
        {
            UrlsList = new List<string>();
            SearchWord = "";

            if (args == null || args.Length==0)
            {
                Console.WriteLine("Provide with arguments");
                Console.WriteLine("Example WebCrawler url1 url2 -s:yourSearchText");
            }
            else
            {

                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    if (argument.StartsWith(@"https://"))
                    {
                        UrlsList.Add(argument);

                    }
                    else if (argument.StartsWith(@"-s:"))
                    {
                        SearchWord = argument.Replace("-s:", "");

                    }
                }
            }


        }
    }
}
