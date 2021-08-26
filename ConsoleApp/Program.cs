using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace ConsoleApp
{
    class Program
    {
        // Report table
        static int tableWidth = 115;
        static void Main(string[] args)
        {
            #region General settings and link helps
            //Use UTF-8 in Console write
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //Get C# version
            //Console.Write(typeof(string).Assembly.ImageRuntimeVersion);

            /* Help links: 
                - ToString() Formats:  https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings 
                - Ansii desing: https://asciiflow.com/#/
                - Relative Path and executable path: https://ourcodeworld.com/articles/read/935/how-to-retrieve-the-executable-path-of-a-console-application-in-c-sharp
                                                     https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getdirectoryname?view=net-5.0
                - Download File: https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient.downloadfile?view=net-5.0
                - Show progress: https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient.downloadprogresschanged?view=net-5.0
                - Async Download: https://docs.microsoft.com/en-us/dotnet/api/system.net.webclient.downloadfileasync?view=net-5.0
                - Network Tracing in the .Net Framework: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/network-tracing
                - Check valid URI: https://newbedev.com/c-how-can-i-check-if-a-url-exists-is-valid
                - Unzip file : https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile.extracttodirectory?view=net-5.0
                - Unzip Gzip: https://stackoverflow.com/questions/24138373/unzip-gz-file-using-c-sharp
                - Read using UTF8 : https://stackoverflow.com/questions/8089357/how-to-read-special-character-like-%C3%A9-%C3%A2-and-others-in-c-sharp
                                    https://newbedev.com/set-c-console-application-to-unicode-output
                - LINQ: https://www.tutorialsteacher.com/linq/linq-query-syntax
                        https://www.tutorialsteacher.com/linq/sample-linq-queries
                        https://stackoverflow.com/questions/41913846/linq-group-by-and-order-by-sum
                        https://stackoverflow.com/questions/5231845/c-sharp-linq-group-by-on-multiple-columns

            */
            #endregion
            Console.WriteLine("┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ CODING ASSIGMENT v.1.0                                                                                             │");
            Console.WriteLine("│ ────────────────────────────────────────────────────────────────────────────────────────────────────────────────── │");
            Console.WriteLine("│                                                                                    By: Jose Enrique Aguirre Chavez │");
            Console.WriteLine("│ Description:                                                                                                       │");
            Console.WriteLine("│ This Console App download last N hours Wikipedia Pageviews from public repository to be analyzed in a top report   │");
            Console.WriteLine("│ https://dumps.wikimedia.org/other/pageviews/, the process has different phases to guarantee the process.           │");
            Console.WriteLine("│ We are using LINQ querys to get the task requirements                                                              │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘\n");
            Console.WriteLine("┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Phase 1: Generate Pageview links, verify in local repository and download .gz files not found                      │"); 
            Console.WriteLine("│ WARNING: Each download takes 2 min per file.                                                                       │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘");
            #region Links generation
            List<PageViewFile> links = getLinks(DateTime.Now, 5);
            #endregion
            #region Download files and load data into List
            //Get local Pageviews repository
            string currentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string file = System.IO.Path.Combine(currentDirectory, @"..\..\..\Pageviews");
            string filePath = System.IO.Path.GetFullPath(file);
            //Verify if we have .gz file in Pageviewrepository - optimal if we gonna change funcion to N hours last and avoid download twice the same file
            Console.WriteLine(" 1.1 Generate links and verify in local repository");
            links = verifyFile(filePath, links);
            foreach (PageViewFile item in links)
            {
                string status = item.found ? "[X]" : "[ ]";
                Console.WriteLine("  |- " + status + " " + item.filename + ".gz");
            }
            //Calculate not found with LINQ Methods
            int total = links.Count(x => x.found == false);
            Console.WriteLine("\n 1.2 Download "+ total + " file(s) not found");
            //Download .gz file
            foreach (PageViewFile item in links)
            {
                if (item.found == false)
                {
                    Console.Write("  |- Downloading " + item.filename + ".gz ...");
                    downloadFiles(item.link, filePath + "\\" + item.filename + ".gz");
                }
            }
            Console.WriteLine("┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Phase 2: Unzip .gz file and load data into list                                                                    │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘");
            Console.WriteLine(" 2.1 Unzip .gz files");
            //Unzip .gz file
            foreach (PageViewFile item in links)
            {
                Console.Write("  |- Unzipping " + item.filename + ".gz ...");
                unzipFiles(item.filename + ".gz", filePath);
            }
            //Load data into List
            Console.WriteLine("\n 2.2 Load data into list");
            //Delete pageview file
            List<PageView> pageviewDB = new List<PageView>();
            foreach (PageViewFile item in links)
            {
                Console.Write("  |- Loading data from " + item.filename + " ...");
                string[] lines = System.IO.File.ReadAllLines(filePath + "\\" + item.filename, System.Text.Encoding.UTF8);
                foreach (string line in lines)
                {
                    string[] textSplit = new string[3];
                    textSplit = line.Split(" ");
                    PageView pageview0 = new PageView();
                    //Some cases file could be corrupt a only have 2 or 1 column 
                    switch (textSplit.Length) {
                        case 1:
                            pageview0.domain_code = textSplit[0];
                            pageview0.page_title = " ";
                            pageview0.count_view = 1;
                            break;
                        case 2:
                            pageview0.domain_code = textSplit[0];
                            pageview0.page_title = textSplit[1];
                            pageview0.count_view = 1;
                            break;
                        default:
                            pageview0.domain_code = textSplit[0];
                            pageview0.page_title = textSplit[1];
                            pageview0.count_view = Int32.Parse(textSplit[2]);
                            break;
                    }
                    pageviewDB.Add(pageview0);
                }
                Console.Write(" Loaded "+lines.Length+" items! \n");
            }
            #endregion
            #region Get task requirements using LINQ queries
            Console.WriteLine("┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Phase 3: Task requeriments - Get Top N Group by Domain_code and Page_tigle Ordered desc by SUM (count_views)       │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘");
            Console.WriteLine(" Input the top number you want to retrieve: ");
            Console.Write(" ");
            var topString = Console.ReadLine();
            while (string.IsNullOrEmpty(topString))
            {
                Console.WriteLine(" Top can't be empty. Please, input once more");
                Console.Write(" ");
                topString = Console.ReadLine();
            }
            int top;
            while (!int.TryParse(topString,out top))
            {
                Console.WriteLine(" Please, enter a a number");
                Console.Write(" ");
                topString = Console.ReadLine();
            }
            Console.WriteLine(" Loading Report Top " + top + " ...");
                      
            var query = (from pageview in pageviewDB
                              group pageview by new 
                              { 
                                  pageview.domain_code,
                                  pageview.page_title
                              } into pageviewGroup
                              orderby pageviewGroup.Sum(x => x.count_view) descending
                              select new { 
                                  Domain = pageviewGroup.Key.domain_code,
                                  Page = pageviewGroup.Key.page_title,
                                  Sum = pageviewGroup.Sum(x=>x.count_view)}
                              ).Take(top);



            PrintLine();
            PrintRow("DOMAIN_CODE", "PAGE_TITLE", "CNT");
            PrintLine();
            foreach (var item in query)
            {
                PrintRow(item.Domain, item.Page, item.Sum.ToString());
            };
            PrintLine();
            Console.WriteLine("\n┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Good Bye: Please press any key to close the program. ♠♥♦♣                                                          │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘");
            Console.ReadLine();
            #endregion
            #region Note Version
            /*
             - Optimize storage, dropping page view and .gz once console app is closed
             - Optimize experience applying menu with optization to chosse N Top report 
             */
            #endregion
            #region NextVersion
            /*
             - Analyze new function last N hours or use a range of datetime 
             - Improve time of download page view (use threads)
             - Improve time retrieve final report (Optimize query o analyze other ways)
             - Put Consoleapp into WebApp (Better performance and recognize UTF-8 characteres)
             */
            #endregion
        }
        #region Report
        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }
        static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
        #endregion
        #region Menu
        static void showMenu()
        {
            Console.WriteLine("\n┌────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Please select one of these options                                                                                 │");
            Console.WriteLine("│    Option 1:  │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┘");
            //Variables
            List<PageViewFile> links0 = new List<PageViewFile>();
            string urlbase = "https://dumps.wikimedia.org/other/pageviews";
            DateTime date0 = new DateTime(startdatetime.Year, startdatetime.Month, startdatetime.Day, startdatetime.Hour, 0, 0);
            //Generate links
            int j = 0;
            for (int i = 0; i < hours; i++)
            {
                PageViewFile item = new PageViewFile();
                if (i == 0)
                {
                    item = linkGenerator(urlbase, date0);
                    //If there no exist .gz file yet, take one hour before .gz file as first link
                    if (validateLink(item.link) == false)
                    {
                        j = j + 1;
                        DateTime date1 = date0.AddHours(-j);
                        item = linkGenerator(urlbase, date1);
                    }
                }
                else
                {
                    j = j + 1;
                    DateTime date2 = date0.AddHours(-j);
                    item = linkGenerator(urlbase, date2);
                }
                links0.Add(item);
            }
            return links0;
        }
        #endregion
        #region Links generation - Methods
        static List<PageViewFile> getLinks(DateTime startdatetime, int hours)
        {
            //Variables
            List<PageViewFile> links0 = new List<PageViewFile>();
            string urlbase = "https://dumps.wikimedia.org/other/pageviews";
            DateTime date0 = new DateTime(startdatetime.Year, startdatetime.Month, startdatetime.Day, startdatetime.Hour, 0, 0);
            //Generate links
            int j = 0;
            for (int i = 0; i < hours; i++)
            {
                PageViewFile item = new PageViewFile();
                if (i == 0)
                {
                    item = linkGenerator(urlbase, date0);
                    //If there no exist .gz file yet, take one hour before .gz file as first link
                    if (validateLink(item.link) == false)
                    {
                        j = j + 1;
                        DateTime date1 = date0.AddHours(-j);
                        item = linkGenerator(urlbase, date1);
                    }
                }
                else
                {
                    j = j + 1;
                    DateTime date2 = date0.AddHours(-j);
                    item = linkGenerator(urlbase, date2);
                }
                links0.Add(item);
            }
            return links0;
        }
        static PageViewFile linkGenerator(string baseurl, DateTime datehour)
        {
            PageViewFile item = new PageViewFile();
            string year = datehour.Year.ToString("0000");
            string month = datehour.Month.ToString("00");
            string day = datehour.Day.ToString("00");
            string hour = datehour.Hour.ToString("00");
            item.filename = "pageviews-" + year + month + day + "-" + hour + "0000";
            item.link = baseurl + "/" + year + "/" + year + "-" + month + "/" + item.filename + ".gz";
            return item;
        }
        static bool validateLink(string url)
        {
            bool result = false;
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                int statusCode = (int)response.StatusCode;
                if (statusCode >= 100 && statusCode < 400) //Good requests
                {
                    result = true;
                }
                else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                {
                    //log.Warn(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                    Console.WriteLine(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                }
                return result;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
        #region Download files and load data into List - Methods
        static List<PageViewFile> verifyFile(string repositorypath, List<PageViewFile> links)
        {
            List<PageViewFile> linksupdated = links;
            foreach (PageViewFile item in linksupdated)
            {
                try
                {
                    string[] files = Directory.GetFiles(repositorypath, item.filename + ".gz");
                    if (files.Length > 0)
                        item.found = true;
                    else
                        item.found = false;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return linksupdated;
        }
        static void downloadFiles(string link, string path) {
            WebClient myWebClient = new WebClient();
            Uri uri = new Uri(link);
            try
            {
                myWebClient.DownloadFile(uri, path);
                Console.Write(" Downloaded!\n");
            }
            catch (WebException e)
            {
                //Console.WriteLine(e.Message);
                Console.Write(" Error!\n");
            }
        }
        static void unzipFiles(string filename, string path)
        {
            DirectoryInfo directorySelected = new DirectoryInfo(path);
            foreach (FileInfo fileToDecompress in directorySelected.GetFiles(filename))
            {
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    {
                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                            Console.Write(" Decompressed! \n");
                        }
                    }
                }
            }
        }
        private static void DownloadProgressCallback4(object sender, DownloadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress.
            Console.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
                (string)e.UserState,
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }
        private static void DownloadFileCallback2(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("File download cancelled.");
            }

            if (e.Error != null)
            {
                Console.WriteLine(e.Error.ToString());
            }
        }
        #endregion
    }
}
