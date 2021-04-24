using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Linq;

namespace WebSiteChecker
{
    class Program
    {
        public string UserUrl;
        public string FormationUrl;

        public List<string> Urls;
        public List<string> UrlsSM;

        public List<CUrl> UrlList;

        static void Main(string[] args)
        {
            Program obj = new Program();
            obj.Urls = new List<string>();
            obj.UrlsSM = new List<string>();
            obj.UrlList = new List<CUrl>();

            Console.Write("Введите адрес сайта: ");
            obj.UserUrl = Console.ReadLine();
            Console.WriteLine("\n");
            obj.FormationUrl = obj.FormationSiteUrl(obj.UserUrl);

            obj.GetUrlsListWithoutSitemap(obj.FormationUrl);
            obj.GetUrlsListWithSitemap(obj.FormationUrl);

            Console.WriteLine("*** output list with urls which exists in sitemap and doesn’t on web site pages ***" , Console.ForegroundColor = ConsoleColor.Green);
            Console.ForegroundColor = ConsoleColor.White;
            obj.Print(obj.UrlsSM, obj.Urls);
            Console.WriteLine("-----------------------------------------------------------------------------------\n", Console.ForegroundColor = ConsoleColor.Red);

            Console.WriteLine("*** output list with urls which exists on web site but doesn’t in sitemap.xml ***", Console.ForegroundColor = ConsoleColor.Green);
            Console.ForegroundColor = ConsoleColor.White;
            obj.Print(obj.Urls, obj.UrlsSM);
            Console.WriteLine("-----------------------------------------------------------------------------------\n", Console.ForegroundColor = ConsoleColor.Red);
            
            Console.WriteLine("***  list with url and response ***", Console.ForegroundColor = ConsoleColor.Green);
            Console.ForegroundColor = ConsoleColor.White;
            obj.PrintWithTiming();
            Console.WriteLine("-----------------------------------------------------------------------------------\n", Console.ForegroundColor = ConsoleColor.Red);

            Console.ReadLine();
        }

        string FormationSiteUrl(string data)
        {
            string url = "";
            if (!data.Contains("http"))
            {
                url = "http://" + data;
            }
            else
                url = data;
            return url;
        }

        void GetUrlsListWithoutSitemap(string url)
        {
            string data = "";
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();


            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                data = stream.ReadToEnd();
            }

            using (StreamWriter sw = new StreamWriter("./data.txt", false, System.Text.Encoding.Default))
            {
                sw.Write(data);
            }

            string[] array = data.Split();

            for (int i = 0; i < array.Length; i++)
            {
                if(array[i].Contains("href") && array[i].Contains("http"))
                {
                    if (array[i].Contains("'"))
                    {
                        array[i] = array[i].Substring(array[i].IndexOf("'") + 1);
                        array[i] = array[i].Substring(0,array[i].LastIndexOf("'"));

                    }
                    if (array[i].Contains('"'))
                    {
                        array[i] = array[i].Substring(array[i].IndexOf('"') + 1);
                        array[i] = array[i].Substring(0,array[i].LastIndexOf('"'));
                    }

                    if (!array[i].Contains("www"))
                    {
                        if (array[i].Contains("https"))
                            array[i] = array[i].Insert(8, "www.");
                        else
                            array[i] = array[i].Insert(7, "www.");
                    }
                    
                    UrlList.Add(new CUrl(array[i], CheckUrl(array[i])));
                    Urls.Add(array[i]);
                }
            }
        }

        void GetUrlsListWithSitemap(string url)
        {
            string data = "";

            WebRequest request = WebRequest.Create(url+"/robots.txt");
            WebResponse response = request.GetResponse();

            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                data = stream.ReadToEnd();
            }

            if(data.Contains("sitemap.xm"))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(url + "/sitemap.xml"), "./data.txt");
                
                using (StreamReader stream = new StreamReader("./data.txt"))
                {
                    data = stream.ReadToEnd();
                }
            }

           string[] array = data.Split();

           for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("<loc>"))
                {
                    if (array[i].Contains("<"))
                    {
                        array[i] = array[i].Substring(array[i].IndexOf(">") + 1);
                        array[i] = array[i].Substring(0, array[i].LastIndexOf("<"));

                    }

                    if (!array[i].Contains("www"))
                    {
                        if (array[i].Contains("https"))
                            array[i] = array[i].Insert(8, "www.");
                        else
                            array[i] = array[i].Insert(7, "www.");
                    }
                    
                    UrlList.Add(new CUrl(array[i], CheckUrl(array[i])));
                    UrlsSM.Add(array[i]); 
                }
            }
        }

        void Print(List<string> flist, List<string> slist)
        {
            foreach (var fitem in flist)
            {
                foreach (var sitem in slist)
                {
                    if (fitem != sitem)
                        break;
                }
                Console.WriteLine(fitem);
            }
        }

        void PrintWithTiming()
        {
            var result = from url in UrlList
                         orderby url.Timing
                         select url;

            foreach (var item in result)
            {
                Console.WriteLine($"{item.Url} : {item.Timing}ms");
            }
        }

        double CheckUrl(string url)
        {
            Stopwatch sw = new Stopwatch();
            WebRequest request = WebRequest.Create(url);
            sw.Start();
            try { WebResponse response = request.GetResponse(); }
            catch { }
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }
    }
}
